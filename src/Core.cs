using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace FileDedup
{
    public class FileItem
    {
        public string FileName;
        public string FullPath;
        public long Length;
        public DateTime Modified;
        public DateTime Created;
        public bool SelectedForDelete;
        public string Note = "";
        public ImageMeta Meta;
        public int Similarity = 100;

        public string SizeText
        {
            get { return Fmt.Size(Length); }
        }

        public string ModifiedText
        {
            get { return Modified.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public string CreatedText
        {
            get { return Created.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
    }

    public class DupGroup
    {
        public int Id;
        public long Size;
        public List<FileItem> Files = new List<FileItem>();
        public bool Similar = false;      // true = 相似图片（感知哈希）分组
        public int Similarity = 100;      // 组内相似度百分比

        public bool IsImageGroup()
        {
            if (Files.Count == 0) return false;
            return ImageHash.IsImage(Files[0].FullPath);
        }

        public DateTime NewestTime()
        {
            DateTime t = DateTime.MinValue;
            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].Modified > t) t = Files[i].Modified;
            }
            return t;
        }

        public string FirstPath()
        {
            if (Files.Count == 0) return "";
            return Files[0].FullPath;
        }
    }

    public static class Fmt
    {
        public static string Size(long b)
        {
            if (b < 1024) return b + " B";
            double kb = b / 1024.0;
            if (kb < 1024) return string.Format("{0:F1} KB", kb);
            double mb = kb / 1024.0;
            if (mb < 1024) return string.Format("{0:F2} MB", mb);
            double gb = mb / 1024.0;
            if (gb < 1024) return string.Format("{0:F2} GB", gb);
            return string.Format("{0:F2} TB", gb / 1024.0);
        }

        public static string Number(int n)
        {
            return n.ToString("N0");
        }
    }

    public static class Hash
    {
        private static string Hex(byte[] b)
        {
            StringBuilder sb = new StringBuilder(b.Length * 2);
            for (int i = 0; i < b.Length; i++) sb.Append(b[i].ToString("x2"));
            return sb.ToString();
        }

        private static string Md5Hex(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                return Hex(md5.ComputeHash(data));
            }
        }

        // 取样指纹：头部 8KB（小文件即全文）+ 尾部 4KB
        public static string Quick(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 8192, FileOptions.None))
            {
                long len = fs.Length;
                int want = (int)Math.Min(8192L, len);
                if (want <= 0) return "0:empty";
                byte[] head = new byte[want];
                int got = 0;
                while (got < want)
                {
                    int r = fs.Read(head, got, want - got);
                    if (r <= 0) break;
                    got += r;
                }
                if (got < want)
                {
                    byte[] cut = new byte[got];
                    Array.Copy(head, cut, got);
                    head = cut;
                }
                string h1 = Md5Hex(head);
                if (len <= 8192) return h1 + ":whole";

                int tailWant = (int)Math.Min(4096L, len);
                byte[] tail = new byte[tailWant];
                fs.Position = len - tailWant;
                int tg = 0;
                while (tg < tailWant)
                {
                    int r = fs.Read(tail, tg, tailWant - tg);
                    if (r <= 0) break;
                    tg += r;
                }
                if (tg < tailWant)
                {
                    byte[] cut2 = new byte[tg];
                    Array.Copy(tail, cut2, tg);
                    tail = cut2;
                }
                return h1 + ":" + Md5Hex(tail);
            }
        }

        public static string Full(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 20, FileOptions.SequentialScan))
            {
                using (SHA256 sha = SHA256.Create())
                {
                    return Hex(sha.ComputeHash(fs));
                }
            }
        }
    }

    public class ScanOptions
    {
        public List<string> Folders = new List<string>();
        public Func<string, bool> AcceptExtension;
        public long MinSize = 1024;
        public bool Recursive = true;
        // 子目录层级：-1 不限；0 只扫当前目录；1 含 1 层子目录，依此类推
        public int MaxDepth = -1;
        public bool SkipHiddenSystem = true;
        public bool SkipZeroByte = true;
        // 相似图片（感知哈希）查重
        public bool SimilarImages = false;
        // 汉明距离阈值：3 严格 / 6 标准 / 12 宽松（64 位指纹）
        public int SimilarThreshold = 6;
    }

    public class ScanSummary
    {
        public List<DupGroup> Groups = new List<DupGroup>();
        public List<DupGroup> SimilarGroups = new List<DupGroup>();
        public int HashedImages;
        public int FilesScanned;
        public int Unreadable;
        public double Seconds;
        public long DupFiles;
        public long WastedBytes;
        public long TotalBytes;      // 1.4：本次扫描到的全部文件总大小，用于统计报告
        public string ModeText = ""; // 1.4：扫描模式说明（如“常规模式 · 相同内容(SHA-256)”）
    }

    // 扫描暂停控制：Pause() 之后工作线程在这里挂起，几乎不占 CPU；Resume() 继续
    public class PauseGate
    {
        private ManualResetEventSlim _running = new ManualResetEventSlim(true);

        public bool Paused { get { return !_running.IsSet; } }

        public void Pause() { _running.Reset(); }

        public void Resume() { _running.Set(); }

        public void Wait(CancellationToken token)
        {
            while (!_running.IsSet)
            {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                try
                {
                    _running.Wait(40, token);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }
            if (token.IsCancellationRequested) throw new OperationCanceledException(token);
        }
    }

    public class Scanner
    {
        private Action<string, int, int> _report;
        private DateTime _lastReport = DateTime.MinValue;
        private PauseGate _gate;

        private void WaitIfPaused(CancellationToken token)
        {
            if (_gate != null) _gate.Wait(token);
        }

        private void Report(string text, int cur, int total, bool force)
        {
            if (_report == null) return;
            DateTime now = DateTime.Now;
            if (!force && (now - _lastReport).TotalMilliseconds < 120) return;
            _lastReport = now;
            _report(text, cur, total);
        }

        public ScanSummary Scan(ScanOptions opt, CancellationToken token, Action<string, int, int> report)
        {
            return Scan(opt, token, null, report);
        }

        public ScanSummary Scan(ScanOptions opt, CancellationToken token, PauseGate gate, Action<string, int, int> report)
        {
            _report = report;
            _gate = gate;
            Stopwatch sw = Stopwatch.StartNew();
            ScanSummary sum = new ScanSummary();

            WaitIfPaused(token);
            Report(Lang.T("正在遍历文件夹…"), 0, 0, true);
            List<FileItem> files = Enumerate(opt, token, sum);
            sum.FilesScanned = files.Count;
            long totalBytes = 0;
            for (int i = 0; i < files.Count; i++) totalBytes += files[i].Length;
            sum.TotalBytes = totalBytes;
            sum.ModeText = (opt.SimilarImages ? "相似图片 + 内容查重" : "常规模式 · 相同内容(SHA-256)");
            if (token.IsCancellationRequested) throw new OperationCanceledException(token);

            if (files.Count == 0)
            {
                sum.Seconds = sw.Elapsed.TotalSeconds;
                Report("扫描完成", 1, 1, true);
                return sum;
            }

            Report(Lang.F("共找到 {0} 个文件，正在按大小分组…", Fmt.Number(files.Count)), 0, 0, true);

            Dictionary<long, List<FileItem>> bySize = new Dictionary<long, List<FileItem>>();
            for (int i = 0; i < files.Count; i++)
            {
                FileItem f = files[i];
                List<FileItem> bucket;
                if (!bySize.TryGetValue(f.Length, out bucket))
                {
                    bucket = new List<FileItem>();
                    bySize[f.Length] = bucket;
                }
                bucket.Add(f);
            }

            List<List<FileItem>> cand = new List<List<FileItem>>();
            int candCount = 0;
            foreach (KeyValuePair<long, List<FileItem>> kv in bySize)
            {
                if (kv.Value.Count >= 2)
                {
                    cand.Add(kv.Value);
                    candCount += kv.Value.Count;
                }
            }
            bySize.Clear();

            if (cand.Count == 0)
            {
                sum.Seconds = sw.Elapsed.TotalSeconds;
                Report("扫描完成", 1, 1, true);
                return sum;
            }

            Report(Lang.F("大小相同的候选文件 {0} 个，正在比对内容指纹…", Fmt.Number(candCount)), 0, candCount, true);

            List<List<FileItem>> fp = new List<List<FileItem>>();
            int done = 0;
            foreach (List<FileItem> g in cand)
            {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                Dictionary<string, List<FileItem>> map = new Dictionary<string, List<FileItem>>();
                foreach (FileItem f in g)
                {
                    WaitIfPaused(token);
                    done++;
                    string h;
                    try
                    {
                        h = Hash.Quick(f.FullPath);
                    }
                    catch
                    {
                        sum.Unreadable++;
                        continue;
                    }
                    List<FileItem> b;
                    if (!map.TryGetValue(h, out b))
                    {
                        b = new List<FileItem>();
                        map[h] = b;
                    }
                    b.Add(f);
                    Report(Lang.T("正在比对内容指纹…"), done, candCount, false);
                }
                foreach (KeyValuePair<string, List<FileItem>> kv in map)
                {
                    if (kv.Value.Count >= 2) fp.Add(kv.Value);
                }
            }
            cand.Clear();

            List<List<FileItem>> needFull = new List<List<FileItem>>();
            List<List<FileItem>> exact = new List<List<FileItem>>();
            int fullCount = 0;
            foreach (List<FileItem> g in fp)
            {
                if (g[0].Length <= 8192)
                {
                    exact.Add(g);
                }
                else
                {
                    needFull.Add(g);
                    fullCount += g.Count;
                }
            }

            if (fullCount > 0)
            {
                Report(string.Format("需要精确校验 {0} 个文件…", Fmt.Number(fullCount)), 0, fullCount, true);
                int hdone = 0;
                foreach (List<FileItem> g in needFull)
                {
                    if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                    Dictionary<string, List<FileItem>> map = new Dictionary<string, List<FileItem>>();
                    foreach (FileItem f in g)
                    {
                        WaitIfPaused(token);
                        hdone++;
                        string h;
                        try
                        {
                            h = Hash.Full(f.FullPath);
                        }
                        catch
                        {
                            sum.Unreadable++;
                            continue;
                        }
                        List<FileItem> b;
                        if (!map.TryGetValue(h, out b))
                        {
                            b = new List<FileItem>();
                            map[h] = b;
                        }
                        b.Add(f);
                        Report(Lang.F("正在精确校验：{0}", f.FileName), hdone, fullCount, false);
                    }
                    foreach (KeyValuePair<string, List<FileItem>> kv in map)
                    {
                        if (kv.Value.Count >= 2) exact.Add(kv.Value);
                    }
                }
            }

            int gid = 0;
            foreach (List<FileItem> g in exact)
            {
                gid++;
                DupGroup dg = new DupGroup();
                dg.Id = gid;
                dg.Size = g[0].Length;
                dg.Files = g;
                sum.Groups.Add(dg);
                sum.DupFiles += g.Count;
                sum.WastedBytes += dg.Size * (long)(g.Count - 1);
            }

            // 相似图片（感知哈希）：排除已精确重复的文件，只看“画面像、但内容不同”
            if (opt.SimilarImages)
            {
                HashSet<FileItem> exactFiles = new HashSet<FileItem>();
                for (int i = 0; i < exact.Count; i++)
                {
                    for (int j = 0; j < exact[i].Count; j++) exactFiles.Add(exact[i][j]);
                }
                RunSimilar(files, opt, sum, exactFiles, token);
            }

            sum.Seconds = sw.Elapsed.TotalSeconds;
            Report("扫描完成", 1, 1, true);
            return sum;
        }

        // 相似图片：算 64 位感知哈希 → BK 树找“汉明距离 ≤ 阈值”的邻居 → 并查集成组
        private void RunSimilar(List<FileItem> files, ScanOptions opt, ScanSummary sum, HashSet<FileItem> exclude, CancellationToken token)
        {
            List<FileItem> cand = new List<FileItem>();
            for (int i = 0; i < files.Count; i++)
            {
                FileItem f = files[i];
                if (exclude.Contains(f)) continue;
                if (!ImageHash.IsImage(f.FullPath)) continue;
                cand.Add(f);
            }
            if (cand.Count < 2) return;

            Report(Lang.F("正在计算图片指纹（共 {0} 张）…", Fmt.Number(cand.Count)), 0, cand.Count, true);

            List<FileItem> ok = new List<FileItem>();
            List<ulong> hashes = new List<ulong>();
            for (int i = 0; i < cand.Count; i++)
            {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                WaitIfPaused(token);
                FileItem f = cand[i];
                ImageMeta m = ImageHash.Read(f.FullPath, true);
                f.Meta = m;
                if (m.HashOk)
                {
                    ok.Add(f);
                    hashes.Add(m.PHash);
                }
                Report(Lang.F("正在计算图片指纹：{0}", f.FileName), i + 1, cand.Count, false);
            }
            sum.HashedImages = ok.Count;
            if (ok.Count < 2) return;

            Report(Lang.F("正在比对画面相似度（阈值 {0}/64）…", opt.SimilarThreshold.ToString()), 0, ok.Count, true);

            PHashIndex idx = new PHashIndex();
            for (int i = 0; i < ok.Count; i++) idx.Add(hashes[i], i);

            int[] parent = new int[ok.Count];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;

            List<int> found = new List<int>();
            for (int i = 0; i < ok.Count; i++)
            {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                if (i % 50 == 0) WaitIfPaused(token);
                found.Clear();
                idx.Query(hashes[i], opt.SimilarThreshold, found);
                for (int k = 0; k < found.Count; k++)
                {
                    int j = found[k];
                    if (j <= i) continue;
                    int a = FindRoot(parent, i), b = FindRoot(parent, j);
                    if (a != b) parent[b] = a;
                }
                Report(Lang.T("正在比对画面相似度…"), i + 1, ok.Count, false);
            }

            Dictionary<int, List<int>> sets = new Dictionary<int, List<int>>();
            for (int i = 0; i < ok.Count; i++)
            {
                int r = FindRoot(parent, i);
                List<int> lst;
                if (!sets.TryGetValue(r, out lst))
                {
                    lst = new List<int>();
                    sets[r] = lst;
                }
                lst.Add(i);
            }

            int gid = sum.Groups.Count;
            foreach (KeyValuePair<int, List<int>> kv in sets)
            {
                List<int> ids = kv.Value;
                if (ids.Count < 2) continue;
                DupGroup g = new DupGroup();
                g.Similar = true;
                gid++;
                g.Id = gid;
                long maxLen = 0;
                int rep = ids[0];
                for (int i = 0; i < ids.Count; i++)
                {
                    FileItem f = ok[ids[i]];
                    if (f.Meta != null && f.Meta.Width * (long)f.Meta.Height > (ok[rep].Meta != null ? ok[rep].Meta.Width * (long)ok[rep].Meta.Height : 0))
                        rep = ids[i];
                    if (f.Length > maxLen) maxLen = f.Length;
                }
                int minSim = 100;
                for (int i = 0; i < ids.Count; i++)
                {
                    FileItem f = ok[ids[i]];
                    int d = ImageHash.Hamming(hashes[rep], hashes[ids[i]]);
                    f.Similarity = 100 - (int)Math.Round(d * 100.0 / 64.0);
                    if (f.Similarity < minSim) minSim = f.Similarity;
                    g.Files.Add(f);
                }
                g.Similarity = minSim;
                g.Size = maxLen;
                sum.SimilarGroups.Add(g);
            }

            sum.SimilarGroups.Sort(delegate(DupGroup a, DupGroup b)
            {
                if (a.Files.Count != b.Files.Count) return b.Files.Count - a.Files.Count;
                return b.Similarity - a.Similarity;
            });
        }

        private static int FindRoot(int[] parent, int i)
        {
            while (parent[i] != i)
            {
                parent[i] = parent[parent[i]];
                i = parent[i];
            }
            return i;
        }

        private List<FileItem> Enumerate(ScanOptions opt, CancellationToken token, ScanSummary sum)
        {
            List<FileItem> list = new List<FileItem>();
            // 栈里同时记目录层级：根目录为 0，子目录 +1
            Stack<KeyValuePair<string, int>> stack = new Stack<KeyValuePair<string, int>>();
            for (int i = 0; i < opt.Folders.Count; i++)
            {
                string fd = opt.Folders[i];
                if (Directory.Exists(fd)) stack.Push(new KeyValuePair<string, int>(fd, 0));
            }
            int dirs = 0;
            while (stack.Count > 0)
            {
                WaitIfPaused(token);
                string dir = stack.Peek().Key;
                int depth = stack.Peek().Value;
                stack.Pop();
                dirs++;
                if (dirs % 60 == 0)
                {
                    Report(Lang.F("正在遍历… 已查 {0} 个文件夹，找到 {1} 个文件", Fmt.Number(dirs), Fmt.Number(list.Count)), 0, 0, false);
                }

                string[] fs = null;
                try
                {
                    fs = Directory.GetFiles(dir);
                }
                catch
                {
                    fs = null;
                }
                if (fs != null)
                {
                    for (int i = 0; i < fs.Length; i++)
                    {
                        WaitIfPaused(token);
                        string fp = fs[i];
                        FileInfo fi;
                        try
                        {
                            fi = new FileInfo(fp);
                        }
                        catch
                        {
                            continue;
                        }
                        long len = fi.Length;
                        if (opt.SkipZeroByte && len == 0) continue;
                        if (len < opt.MinSize) continue;

                        FileAttributes at;
                        try
                        {
                            at = fi.Attributes;
                        }
                        catch
                        {
                            continue;
                        }
                        if (opt.SkipHiddenSystem)
                        {
                            if ((at & FileAttributes.Hidden) != 0) continue;
                            if ((at & FileAttributes.System) != 0) continue;
                        }
                        if ((at & FileAttributes.Directory) != 0) continue;

                        string ext = fi.Extension;
                        if (opt.AcceptExtension != null && !opt.AcceptExtension(ext)) continue;

                        FileItem it = new FileItem();
                        it.FileName = fi.Name;
                        it.FullPath = fi.FullName;
                        it.Length = len;
                        try
                        {
                            it.Modified = fi.LastWriteTime;
                        }
                        catch
                        {
                            it.Modified = DateTime.MinValue;
                        }
                        // 1.4：创建时间（部分文件系统/网络盘取不到，取不到就退化为修改时间）
                        try
                        {
                            it.Created = fi.CreationTime;
                            if (it.Created <= new DateTime(1601, 1, 1)) it.Created = it.Modified;
                        }
                        catch
                        {
                            it.Created = it.Modified;
                        }
                        list.Add(it);
                    }
                }

                if (!opt.Recursive) continue;
                int maxDepth = opt.Recursive ? opt.MaxDepth : 0;
                if (maxDepth >= 0 && depth >= maxDepth) continue;

                string[] subs = null;
                try
                {
                    subs = Directory.GetDirectories(dir);
                }
                catch
                {
                    subs = null;
                }
                if (subs == null) continue;
                for (int i = 0; i < subs.Length; i++)
                {
                    string sd = subs[i];
                    string nm = Path.GetFileName(sd);
                    if (nm == "$RECYCLE.BIN" || nm == "System Volume Information") continue;
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(sd);
                        FileAttributes dat = di.Attributes;
                        if ((dat & FileAttributes.ReparsePoint) != 0) continue;
                        if (opt.SkipHiddenSystem)
                        {
                            if ((dat & FileAttributes.Hidden) != 0) continue;
                            if ((dat & FileAttributes.System) != 0) continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                    stack.Push(new KeyValuePair<string, int>(sd, depth + 1));
                }
            }
            return list;
        }
    }
}


