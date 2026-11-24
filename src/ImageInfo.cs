using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace FileDedup
{
    // 图片元信息（尺寸 / 拍摄参数 / 感知哈希）
    public class ImageMeta
    {
        public int Width;
        public int Height;
        public int Bits;
        public int Iso;
        public DateTime Taken = DateTime.MinValue;
        public string Make = "";
        public string Model = "";
        public string Lens = "";
        public string ColorSpace = "";
        public ulong PHash;
        public bool HashOk;
        public string Error = "";

        public string DimText
        {
            get { return (Width > 0 && Height > 0) ? (Width + "×" + Height) : ""; }
        }

        public string CameraText
        {
            get
            {
                string s = (Make + " " + Model).Trim();
                return s.Length > 0 ? s : "";
            }
        }

        public string TakenText
        {
            get { return Taken == DateTime.MinValue ? "" : Taken.ToString("yyyy-MM-dd HH:mm"); }
        }
    }

    public static class ImageHash
    {
        // 参与“相似图片”比对的扩展名（GDI+ 能解码的直接解，RAW 走内嵌预览）
        public static readonly string[] Exts = new string[] {
            ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".apng", ".gif", ".bmp", ".dib",
            ".tif", ".tiff", ".webp", ".ico", ".cur", ".heic", ".heif", ".avif",
            ".cr2", ".cr3", ".crw", ".nef", ".nrw", ".arw", ".sr2", ".srf", ".orf",
            ".rw2", ".rwl", ".raf", ".pef", ".x3f", ".erf", ".mrw", ".dcr", ".k25",
            ".3fr", ".fff", ".iiq", ".dng", ".raw"
        };

        public static bool IsImage(string path)
        {
            string e = Path.GetExtension(path);
            if (e == null) return false;
            e = e.ToLowerInvariant();
            for (int i = 0; i < Exts.Length; i++)
                if (Exts[i] == e) return true;
            return false;
        }

        private static bool IsRaw(string ext)
        {
            return ext == ".cr2" || ext == ".cr3" || ext == ".crw" || ext == ".nef" || ext == ".nrw"
                || ext == ".arw" || ext == ".sr2" || ext == ".srf" || ext == ".orf" || ext == ".rw2"
                || ext == ".rwl" || ext == ".raf" || ext == ".pef" || ext == ".x3f" || ext == ".erf"
                || ext == ".mrw" || ext == ".dcr" || ext == ".k25" || ext == ".3fr" || ext == ".fff"
                || ext == ".iiq" || ext == ".dng" || ext == ".raw";
        }

        // 统一入口：普通图片直接解码；RAW 先抽内嵌 JPEG 预览
        private static Image Load(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (!IsRaw(ext))
                return Image.FromFile(path);

            byte[] jpg = ExtractEmbeddedJpeg(path);
            if (jpg == null) throw new NotSupportedException("no preview");
            MemoryStream ms = new MemoryStream(jpg);
            return Image.FromStream(ms);
        }

        // 在文件前若干 MB 里找最大的 JPEG 段（相机 RAW 都带全尺寸预览）
        private static byte[] ExtractEmbeddedJpeg(string path)
        {
            const int ScanBytes = 12 * 1024 * 1024;
            byte[] buf;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    int want = (int)Math.Min((long)ScanBytes, fs.Length);
                    if (want <= 0) return null;
                    buf = new byte[want];
                    int got = 0;
                    while (got < want)
                    {
                        int r = fs.Read(buf, got, want - got);
                        if (r <= 0) break;
                        got += r;
                    }
                    if (got < want)
                    {
                        byte[] cut = new byte[got];
                        Array.Copy(buf, cut, got);
                        buf = cut;
                    }
                }
            }
            catch
            {
                return null;
            }

            int bestStart = -1, bestLen = 0;
            for (int i = 0; i + 3 < buf.Length; i++)
            {
                if (buf[i] == 0xFF && buf[i + 1] == 0xD8 && buf[i + 2] == 0xFF)
                {
                    int end = -1;
                    for (int j = i + 3; j + 1 < buf.Length; j++)
                    {
                        if (buf[j] == 0xFF && buf[j + 1] == 0xD9)
                        {
                            end = j + 2;
                            break;
                        }
                    }
                    if (end > 0)
                    {
                        int len = end - i;
                        if (len > bestLen)
                        {
                            bestLen = len;
                            bestStart = i;
                        }
                        i = end - 1;
                    }
                }
            }
            if (bestStart < 0 || bestLen < 4096) return null;
            byte[] outBuf = new byte[bestLen];
            Array.Copy(buf, bestStart, outBuf, 0, bestLen);
            return outBuf;
        }

        private static string Str(PropertyItem p)
        {
            try
            {
                if (p == null || p.Value == null) return "";
                byte[] b = (byte[])p.Value;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < b.Length; i++)
                {
                    char c = (char)b[i];
                    if (c == '\0') break;
                    if (c < 32) continue;
                    sb.Append(c);
                }
                return sb.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        private static int Num(PropertyItem p, int def)
        {
            try
            {
                if (p == null || p.Value == null) return def;
                byte[] b = (byte[])p.Value;
                if (b.Length == 0) return def;
                if (p.Type == 3 && b.Length >= 2) return BitConverter.ToUInt16(b, 0);
                if (p.Type == 4 && b.Length >= 4) return (int)BitConverter.ToUInt32(b, 0);
                return def;
            }
            catch
            {
                return def;
            }
        }

        private static PropertyItem Find(Image img, int id)
        {
            try
            {
                PropertyItem[] items = img.PropertyItems;
                for (int i = 0; i < items.Length; i++)
                    if (items[i].Id == id) return items[i];
            }
            catch
            {
            }
            return null;
        }

        private static DateTime ParseExifDate(string s)
        {
            if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
            try
            {
                s = s.Trim();
                // 形如 2024:04:06 17:42:11
                if (s.Length >= 19)
                {
                    int y = int.Parse(s.Substring(0, 4));
                    int m = int.Parse(s.Substring(5, 2));
                    int d = int.Parse(s.Substring(8, 2));
                    int hh = int.Parse(s.Substring(11, 2));
                    int mm = int.Parse(s.Substring(14, 2));
                    int ss = int.Parse(s.Substring(17, 2));
                    return new DateTime(y, m, d, hh, mm, ss);
                }
            }
            catch
            {
            }
            return DateTime.MinValue;
        }

        // 读尺寸 + 拍摄参数；withHash=true 时顺带算指纹（缩略小图，速度快）
        public static ImageMeta Read(string path, bool withHash)
        {
            ImageMeta m = new ImageMeta();
            Image img = null;
            try
            {
                img = Load(path);
                m.Width = img.Width;
                m.Height = img.Height;
                m.Make = Str(Find(img, 0x010F));
                m.Model = Str(Find(img, 0x0110));
                m.Lens = Str(Find(img, 0xA434));
                m.Iso = Num(Find(img, 0x8827), 0);
                if (m.Iso <= 0) m.Iso = Num(Find(img, 0x8833), 0);
                m.Bits = Num(Find(img, 0x0102), 0);
                if (m.Bits <= 0) m.Bits = 8;   // 绝大多数 JPG/PNG 不写这一项，按 8 位/通道处理
                int cs = Num(Find(img, 0xA001), 0);
                m.ColorSpace = (cs == 0xFFFF) ? "AdobeRGB" : "sRGB";
                m.Taken = ParseExifDate(Str(Find(img, 0x9003)));
                if (m.Taken == DateTime.MinValue) m.Taken = ParseExifDate(Str(Find(img, 0x0132)));
                int pw = Num(Find(img, 0xA002), 0);
                int ph = Num(Find(img, 0xA003), 0);
                if (pw > 0 && ph > 0 && (long)pw * ph > (long)m.Width * m.Height)
                {
                    m.Width = pw;
                    m.Height = ph;
                }

                if (withHash)
                {
                    Bitmap small = ToGray32(img);
                    m.PHash = Compute(small);
                    m.HashOk = true;
                    small.Dispose();
                }
            }
            catch (Exception ex)
            {
                m.Error = ex.Message;
                m.HashOk = false;
            }
            finally
            {
                if (img != null) { try { img.Dispose(); } catch { } }
            }
            return m;
        }

        // 32×32 灰度
        private static Bitmap ToGray32(Image src)
        {
            Bitmap bmp = new Bitmap(32, 32, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                int sw = src.Width, sh = src.Height;
                // 先缩到较短边 32 再居中裁成正方形，避免拉伸改变画面结构
                int side = Math.Min(sw, sh);
                Rectangle srcRect = new Rectangle((sw - side) / 2, (sh - side) / 2, side, side);
                g.DrawImage(src, new Rectangle(0, 0, 32, 32), srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        // pHash：32×32 灰度 → 2D DCT → 取左上 8×8（去直流）→ 与中位数比较得 64 位
        private static ulong Compute(Bitmap bmp)
        {
            double[,] pix = new double[32, 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    pix[y, x] = (c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
                }
            }

            double[,] rows = new double[32, 32];
            for (int y = 0; y < 32; y++)
            {
                for (int u = 0; u < 32; u++)
                {
                    double s = 0;
                    for (int x = 0; x < 32; x++)
                        s += pix[y, x] * Math.Cos((2 * x + 1) * u * Math.PI / 64.0);
                    rows[y, u] = s * ((u == 0) ? Math.Sqrt(1.0 / 32) : Math.Sqrt(2.0 / 32));
                }
            }

            double[,] dct = new double[32, 32];
            for (int v = 0; v < 32; v++)
            {
                for (int u = 0; u < 32; u++)
                {
                    double s = 0;
                    for (int y = 0; y < 32; y++)
                        s += rows[y, u] * Math.Cos((2 * y + 1) * v * Math.PI / 64.0);
                    dct[v, u] = s * ((v == 0) ? Math.Sqrt(1.0 / 32) : Math.Sqrt(2.0 / 32));
                }
            }

            double[] vals = new double[63];
            int k = 0;
            for (int v = 0; v < 8; v++)
            {
                for (int u = 0; u < 8; u++)
                {
                    if (v == 0 && u == 0) continue;
                    vals[k++] = dct[v, u];
                }
            }
            Array.Sort(vals);
            double med = vals[vals.Length / 2];

            ulong hash = 0;
            k = 0;
            for (int v = 0; v < 8; v++)
            {
                for (int u = 0; u < 8; u++)
                {
                    if (v == 0 && u == 0) continue;
                    if (dct[v, u] > med) hash |= (1UL << k);
                    k++;
                }
            }
            return hash;
        }

        public static int Hamming(ulong a, ulong b)
        {
            ulong x = a ^ b;
            int n = 0;
            while (x != 0)
            {
                x &= (x - 1);
                n++;
            }
            return n;
        }

        // 界面缩略图（等比放进 w×h 的白底图里）
        public static Bitmap Thumb(string path, int w, int h)
        {
            return Thumb(path, w, h, Color.White);
        }

        public static Bitmap Thumb(string path, int w, int h, Color bg)
        {
            Image img = null;
            try
            {
                img = Load(path);
                int sw = img.Width, sh = img.Height;
                if (sw <= 0 || sh <= 0) return null;
                double k = Math.Min((double)w / sw, (double)h / sh);
                int dw = Math.Max(1, (int)(sw * k));
                int dh = Math.Max(1, (int)(sh * k));
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(bg);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(img, (w - dw) / 2, (h - dh) / 2, dw, dh);
                }
                return bmp;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (img != null) { try { img.Dispose(); } catch { } }
            }
        }
    }

    // 感知哈希索引（BK 树）：比全量两两比对快得多，几千张图毫秒级
    public class PHashIndex
    {
        private class Node
        {
            public ulong Key;
            public List<int> Ids = new List<int>();
            public Dictionary<int, Node> Children = new Dictionary<int, Node>();
        }

        private Node _root;

        public void Add(ulong hash, int id)
        {
            if (_root == null)
            {
                Node n = new Node();
                n.Key = hash;
                n.Ids.Add(id);
                _root = n;
                return;
            }
            Node cur = _root;
            while (true)
            {
                int d = ImageHash.Hamming(hash, cur.Key);
                if (d == 0)
                {
                    cur.Ids.Add(id);
                    return;
                }
                Node child;
                if (!cur.Children.TryGetValue(d, out child))
                {
                    child = new Node();
                    child.Key = hash;
                    child.Ids.Add(id);
                    cur.Children[d] = child;
                    return;
                }
                cur = child;
            }
        }

        public void Query(ulong hash, int radius, List<int> found)
        {
            if (_root == null) return;
            Stack<Node> st = new Stack<Node>();
            st.Push(_root);
            int visited = 0;
            while (st.Count > 0 && visited < 500000)
            {
                visited++;
                Node n = st.Pop();
                int d = ImageHash.Hamming(hash, n.Key);
                if (d <= radius)
                {
                    for (int i = 0; i < n.Ids.Count; i++) found.Add(n.Ids[i]);
                }
                foreach (KeyValuePair<int, Node> c in n.Children)
                {
                    if (c.Key >= d - radius && c.Key <= d + radius) st.Push(c.Value);
                }
            }
        }
    }
}


