using System;
using System.Collections.Generic;

namespace FileDedup
{
    // 多语言：以“简体中文原文”为键，附带繁体 / 英文 / 日文的译文
    internal static class Lang
    {
        public const string ZhCn = "zh-CN";
        public const string ZhTw = "zh-TW";
        public const string En = "en";
        public const string Ja = "ja";
        public const string Ru = "ru";
        public const string Ko = "ko";

        public static string Code = ZhCn;

        private static Dictionary<string, string[]> _map = new Dictionary<string, string[]>();

        private static void D(string zh, string tw, string en, string ja)
        {
            _map[zh] = new string[] { tw, en, ja };
        }

        public static bool IsZhCn { get { return Code == ZhCn; } }

        private static int Index
        {
            get
            {
                if (Code == ZhTw) return 0;
                if (Code == En) return 1;
                if (Code == Ja) return 2;
                return -1;
            }
        }

        // 翻译单条文本；查不到就原样返回
        public static string T(string zh)
        {
            if (string.IsNullOrEmpty(zh)) return zh;

            if (Code == Ru)
            {
                string ru = LangRuKo.Ru(zh);
                if (!string.IsNullOrEmpty(ru)) return ru;
                return Fallback(zh);
            }
            if (Code == Ko)
            {
                string ko = LangRuKo.Ko(zh);
                if (!string.IsNullOrEmpty(ko)) return ko;
                return Fallback(zh);
            }

            int ix = Index;
            if (ix < 0) return zh;
            string[] v;
            if (_map.TryGetValue(zh, out v))
            {
                string s = v[ix];
                if (!string.IsNullOrEmpty(s)) return s;
            }
            return zh;
        }

        // 俄语 / 韩语缺词条时回退到英文，再回退到中文
        private static string Fallback(string zh)
        {
            string[] v;
            if (_map.TryGetValue(zh, out v))
            {
                if (!string.IsNullOrEmpty(v[1])) return v[1];
            }
            return zh;
        }

        // 翻译 + 格式化
        public static string F(string zh, params object[] args)
        {
            string fmt = T(zh);
            try
            {
                return string.Format(fmt, args);
            }
            catch
            {
                return fmt;
            }
        }

        static Lang()
        {
            // ---------- 标题与说明 ----------
            D("文件查重工具", "檔案查重工具", "Duplicate File Finder", "重複ファイル検索ツール");
            D("按文件内容精确比对照片、视频、文档 —— 看清每条路径，按大小和时间排序，安全清理重复副本",
              "依檔案內容精確比對照片、影片、文件 —— 看清每條路徑，依大小與時間排序，安全清理重複副本",
              "Compares photos, videos and documents by exact content - see every path, sort by size and date, clean up duplicates safely",
              "写真・動画・文書を内容で正確に比較 — すべてのパスを確認し、サイズと日付で並べて安全に整理");

            // ---------- 菜单 ----------
            D("文件", "檔案", "File", "ファイル");
            D("视图", "檢視", "View", "表示");
            D("语言", "語言", "Language", "言語");
            D("关于", "關於", "Help", "ヘルプ");
            D("编辑", "編輯", "Edit", "編集");
            D("复制选中路径", "複製選取路徑", "Copy selected paths", "選択したパスをコピー");
            D("全选勾选", "全部勾選", "Check all", "すべてチェック");
            D("清空扫描结果", "清除掃描結果", "Clear results", "結果をクリア");
            D("字体大小", "字級", "Font size", "文字サイズ");
            D("字体设置…", "字型設定…", "Font settings...", "フォント設定…");
            D("导出清单 (CSV)…", "匯出清單 (CSV)…", "Export list (CSV)...", "リストを書き出す (CSV)…");
            D("退出", "結束", "Exit", "終了");
            D("较小", "較小", "Smaller", "小");
            D("标准", "標準", "Default", "標準");
            D("较大", "較大", "Larger", "大");
            D("特大", "特大", "Largest", "特大");
            D("白色主题", "白色主題", "Light theme", "ライトテーマ");
            D("深色主题", "深色主題", "Dark theme", "ダークテーマ");
            D("自定义主题…", "自訂主題…", "Custom theme...", "カスタムテーマ…");
            D("恢复默认背景", "恢復預設背景", "Reset background", "背景をリセット");
            D("版本说明", "版本說明", "Version info", "バージョン情報");
            D("关于软件", "關於軟體", "About the software", "ソフトについて");
            D("关于软件…", "關於軟體…", "About the software...", "ソフトについて…");
            D("检查更新", "檢查更新", "Check for updates", "更新を確認");
            D("关于作者", "關於作者", "About the author", "作者について");
            D("版本与定位", "版本與定位", "What this tool is", "このツールについて");
            D("使用说明（要点）", "使用說明（要點）", "How to use (key points)", "使い方（要点）");
            D("支持的文件格式", "支援的檔案格式", "Supported file formats", "対応ファイル形式");
            D("开发者与主页", "開發者與主頁", "Developer and links", "開発者とリンク");
            D("照片网站", "照片網站", "Photo portfolios", "写真サイト");
            D("B站", "B站", "Bilibili", "Bilibili");
            D("油管", "YouTube", "YouTube", "YouTube");
            D("新片场", "新片場", "Xinpianchang", "Xinpianchang");
            D("微博", "微博", "Weibo", "Weibo");
            D("抖音", "抖音", "Douyin", "Douyin");
            D("小红书", "小紅書", "Xiaohongshu", "Xiaohongshu");
            D("图片（98 种）", "圖片（98 種）", "Images (98 types)", "画像（98 種）");
            D("视频（71 种）", "影片（71 種）", "Videos (71 types)", "動画（71 種）");
            D("文档（110 种）", "文件（110 種）", "Documents (110 types)", "文書（110 種）");
            D("音频（47 种）", "音訊（47 種）", "Audio (47 types)", "音声（47 種）");
            D("压缩包（55 种）", "壓縮檔（55 種）", "Archives (55 types)", "圧縮ファイル（55 種）");
            D("文件查重工具 1.4 是一个免安装的 Windows 单文件查重工具：按文件内容（不是文件名、不是后缀）判断重复，显示每个副本的完整路径，可按大小/时间/路径排序，并可删除到回收站或永久删除；勾选“相似图片”还能按画面比对照片，并并排预览缩略图与拍摄参数。",
              "檔案查重工具 1.4 是一個免安裝的 Windows 單檔查重工具：依檔案內容（不是檔名、不是副檔名）判斷重複，顯示每個副本的完整路徑，可依大小/時間/路徑排序，並可刪除到資源回收筒或永久刪除；勾選「相似圖片」還能依畫面比對照片，並並排預覽縮圖與拍攝參數。",
              "Duplicate File Finder 1.4 is a portable single-file Windows tool: it finds duplicates by file content (not by name or extension), shows every full path, sorts by size / date / path, and deletes to the Recycle Bin or permanently. Tick \"Similar photos\" to compare images by what they look like, with side-by-side thumbnails and shooting data.",
              "重複ファイル検索ツール 1.4 はインストール不要の Windows 単体ツールです。ファイル名や拡張子ではなく内容で重複を判定し、各コピーの完全なパスを表示、サイズ／日時／パスで並べ替え、ごみ箱または完全削除に対応します。「類似写真」をオンにすると画面で写真を比較し、サムネイルと撮影情報を並べて表示します。");
            D("1. 把要检查的文件夹拖进左侧“扫描范围”（可添加多个），或点“添加文件夹”。\n2. 选择文件类型；想连陌生后缀、改过名的文件一起查，勾选“全文件扫描”。\n3. 在“扫描设置”里选择扫描层级（仅当前目录 / 含 1-3 层 / 所有子目录）与最小文件大小。\n4. 点“开始扫描”，扫描过程中可“暂停/继续”，也可以“取消扫描”。\n5. 结果按组显示：勾选要删的副本 → “删除到回收站”或“永久删除”；也可“导出清单”存成 CSV。\n6. 勾选“相似图片（感知哈希）”可选三档阈值（严格/标准/宽松），按画面找相似照片；选中一组，右侧“对比预览”会并排显示缩略图，并对照分辨率、位深色域、拍摄时间、相机与镜头（EXIF）。\n7. 顶部菜单：文件（字体/导出/编辑）、视图（白色/深色/自定义背景）、语言（简中/繁中/English/日本語/Русский/한국어）、关于。",
              "1. 把要檢查的資料夾拖進左側「掃描範圍」（可加入多個），或按「加入資料夾」。\n2. 選擇檔案類型；想連陌生副檔名、改過檔名的檔案一起查，勾選「全檔案掃描」。\n3. 在「掃描設定」裡選擇掃描層級（僅目前目錄 / 含 1-3 層 / 所有子資料夾）與最小檔案大小。\n4. 按「開始掃描」，掃描過程中可「暫停/繼續」，也可以「取消掃描」。\n5. 結果依組顯示：勾選要刪的副本 → 「刪除到資源回收筒」或「永久刪除」；也可「匯出清單」存成 CSV。\n6. 勾選「相似圖片（感知雜湊）」可選三檔閾值（嚴格/標準/寬鬆），依畫面找相似照片；選取一組，右側「對比預覽」會並排顯示縮圖，並對照解析度、位元深度色域、拍攝時間、相機與鏡頭（EXIF）。\n7. 頂部選單：檔案（字型/匯出/編輯）、檢視（白色/深色/自訂背景）、語言（簡中/繁中/English/日本語/Русский/한국어）、關於。",
              "1. Drag folders into \"Scan scope\" on the left (several allowed), or click \"Add folder\".\n2. Pick file types; tick \"All files\" to include unknown extensions and renamed files.\n3. In \"Scan options\" choose the folder depth (this folder / 1-3 levels / all subfolders) and the minimum size.\n4. Click \"Start scan\"; you can Pause / Resume or Cancel while it runs.\n5. Results are grouped: tick the copies to delete, then \"Delete (recycle)\" or \"Delete (permanent)\"; or \"Export list\" to CSV.\n6. Tick \"Similar photos (perceptual hash)\" and pick Strict / Normal / Loose; select a group and the \"Compare preview\" shows thumbnails side by side with resolution, bit depth / colour, capture time, camera and lens (EXIF).\n7. Top menu: File (fonts / export / edit), View (light / dark / custom background), Language (zh-CN / zh-TW / English / Japanese / Russian / Korean), About.",
              "1. 検査するフォルダーを左側の「スキャン範囲」にドラッグ（複数可）、または「フォルダー追加」を押します。\n2. ファイル種別を選びます。未知の拡張子や名前を変えたファイルも対象にするには「全ファイル」をオンにします。\n3. 「スキャン設定」で階層（現在のフォルダーのみ / 1〜3 階層 / すべてのサブフォルダー）と最小サイズを選びます。\n4. 「スキャン開始」を押します。実行中は「一時停止／再開」「スキャン中止」が使えます。\n5. 結果は組ごとに表示されます。削除する副本にチェック → 「ごみ箱へ削除」または「完全に削除」。「リスト出力」で CSV も保存できます。\n6. 「類似写真（知覚ハッシュ）」をオンにし、厳格／標準／緩めを選択。組を選ぶと「比較プレビュー」にサムネイルが並び、解像度・ビット深度／色空間・撮影日時・カメラ・レンズ（EXIF）を比較できます。\n7. 上部メニュー：ファイル（フォント／出力／編集）、表示（ライト／ダーク／カスタム背景）、言語（簡中／繁中／English／日本語／Русский／한국어）、ヘルプ。");
            D("1. 把要检查的文件夹拖进左侧“扫描范围”（可添加多个），或点“添加文件夹”。\n2. 选择文件类型；想连陌生后缀、改过名的文件一起查，勾选“全文件扫描”。\n3. 在“扫描设置”里选择扫描层级（仅当前目录 / 含 1-3 层 / 所有子目录）与最小文件大小。\n4. 点“开始扫描”，扫描过程中可“暂停/继续”，也可以“取消扫描”。\n5. 结果按组显示：勾选要删的副本 → “删除到回收站”或“永久删除”；也可“导出清单”存成 CSV。\n6. 顶部菜单：文件（字体/导出/编辑）、视图（白色/深色/自定义背景）、语言（简中/繁中/English/日本語/Русский/한국어）、关于。",
              "1. 把要檢查的資料夾拖進左側「掃描範圍」（可加入多個），或按「加入資料夾」。\n2. 選擇檔案類型；想連陌生副檔名、改過檔名的檔案一起查，勾選「全檔案掃描」。\n3. 在「掃描設定」裡選擇掃描層級（僅目前目錄 / 含 1-3 層 / 所有子資料夾）與最小檔案大小。\n4. 按「開始掃描」，掃描過程中可「暫停/繼續」，也可以「取消掃描」。\n5. 結果依組顯示：勾選要刪的副本 → 「刪除到資源回收筒」或「永久刪除」；也可「匯出清單」存成 CSV。\n6. 頂部選單：檔案（字型/匯出/編輯）、檢視（白色/深色/自訂背景）、語言（簡中/繁中/English/日本語/Русский/한국어）、關於。",
              "1. Drag folders into \"Scan scope\" on the left (several allowed), or click \"Add folder\".\n2. Pick file types; tick \"All files\" to include unknown extensions and renamed files.\n3. In \"Scan options\" choose the folder depth (this folder / 1-3 levels / all subfolders) and the minimum size.\n4. Click \"Start scan\"; you can Pause / Resume or Cancel while it runs.\n5. Results are grouped: tick the copies to delete, then \"Delete (recycle)\" or \"Delete (permanent)\"; or \"Export list\" to CSV.\n6. Top menu: File (fonts / export / edit), View (light / dark / custom background), Language (zh-CN / zh-TW / English / Japanese / Russian / Korean), About.",
              "1. 検査するフォルダーを左側の「スキャン範囲」にドラッグ（複数可）、または「フォルダー追加」を押します。\n2. ファイル種別を選びます。未知の拡張子や名前を変えたファイルも対象にするには「全ファイル」をオンにします。\n3. 「スキャン設定」で階層（現在のフォルダーのみ / 1〜3 階層 / すべてのサブフォルダー）と最小サイズを選びます。\n4. 「スキャン開始」を押します。実行中は「一時停止／再開」「スキャン中止」が使えます。\n5. 結果は組ごとに表示されます。削除する副本にチェック → 「ごみ箱へ削除」または「完全に削除」。「リスト出力」で CSV も保存できます。\n6. 上部メニュー：ファイル（フォント／出力／編集）、表示（ライト／ダーク／カスタム背景）、言語（簡中／繁中／English／日本語／Русский／한국어）、ヘルプ。");
            D("格式按分类认领（共约 380 种），没列到的可以在“补充扩展名”里自己加；勾选“全文件扫描”则完全不看扩展名。",
              "格式依分類認領（共約 380 種），沒列到的可以在「補充副檔名」裡自行加入；勾選「全檔案掃描」則完全不看副檔名。",
              "About 380 extensions are claimed by category; anything missing can be added under \"Extra extensions\". With \"All files\" the extension is ignored entirely.",
              "約 380 種の拡張子を分類ごとに登録しています。足りないものは「追加の拡張子」で追加できます。「全ファイル」を選ぶと拡張子は一切見ません。");

            // ---------- 左侧 ----------
            D("就绪：添加要扫描的文件夹，然后点“开始扫描”。", "就緒：加入要掃描的資料夾，然後按「開始掃描」。",
              "Ready: add folders to scan, then click Start.", "準備完了：スキャンするフォルダーを追加し、「スキャン開始」を押してください。");
            D("扫描范围（可把文件夹拖进来）", "掃描範圍（可將資料夾拖入）", "Scan scope (drag folders here)", "スキャン範囲（フォルダーをドラッグ可）");
            D("添加文件夹", "加入資料夾", "Add folder", "フォルダー追加");
            D("移除选中", "移除選取", "Remove", "選択を削除");
            D("清空", "清空", "Clear", "クリア");
            D("文件类型", "檔案類型", "File types", "ファイル種別");
            D("图片", "圖片", "Images", "画像");
            D("视频", "影片", "Videos", "動画");
            D("文档", "文件", "Documents", "文書");
            D("音频", "音訊", "Audio", "音声");
            D("压缩包", "壓縮檔", "Archives", "圧縮ファイル");
            D("其他文件", "其他檔案", "Other files", "その他");
            D("补充扩展名（可选，逗号分隔）", "補充副檔名（選填，逗號分隔）", "Extra extensions (optional, comma separated)", "追加の拡張子（任意、カンマ区切り）");
            D("扫描设置", "掃描設定", "Scan options", "スキャン設定");
            D("全文件扫描", "全檔案掃描", "All files", "全ファイル");
            D("扫描层级", "掃描層級", "Scan depth", "スキャン階層");
            D("仅当前目录（不含子目录）", "僅目前目錄（不含子目錄）", "This folder only (no subfolders)", "現在のフォルダーのみ（サブフォルダーなし）");
            D("含 1 层子目录", "含 1 層子資料夾", "Include 1 subfolder level", "サブフォルダー 1 階層を含む");
            D("含 2 层子目录", "含 2 層子資料夾", "Include 2 subfolder levels", "サブフォルダー 2 階層を含む");
            D("含 3 层子目录", "含 3 層子資料夾", "Include 3 subfolder levels", "サブフォルダー 3 階層を含む");
            D("所有子目录（不限层级）", "所有子資料夾（不限層級）", "All subfolders (no limit)", "全サブフォルダー（制限なし）");
            D("包含子文件夹", "包含子資料夾", "Include subfolders", "サブフォルダーを含む");
            D("跳过隐藏文件与系统目录", "略過隱藏檔案與系統目錄", "Skip hidden files and system folders", "隠しファイルとシステムフォルダーを除外");
            D("忽略 0 字节空文件", "忽略 0 位元組空檔案", "Ignore empty (0-byte) files", "0 バイトの空ファイルを無視");
            D("最小文件大小", "最小檔案大小", "Minimum file size", "最小ファイルサイズ");
            D("MB（0 = 不限）", "MB（0 = 不限）", "MB (0 = no limit)", "MB（0 = 制限なし）");
            D("开始扫描", "開始掃描", "Start scan", "スキャン開始");
            D("暂停", "暫停", "Pause", "一時停止");
            D("继续", "繼續", "Resume", "再開");
            D("取消扫描", "取消掃描", "Cancel scan", "スキャン中止");
            D("已暂停扫描（点“继续”接着扫）。", "已暫停掃描（點「繼續」接著掃）。", "Scan paused - click Resume to continue.", "スキャンを一時停止しました（「再開」で続行）。");
            D("已继续扫描…", "已繼續掃描…", "Scan resumed...", "スキャンを再開しました…");
            D("正在取消扫描…", "正在取消掃描…", "Cancelling scan...", "スキャンを中止しています…");
            D("已取消扫描。", "已取消掃描。", "Scan cancelled.", "スキャンを中止しました。");
            D("停止", "停止", "Stop", "停止");

            // ---------- 结果区 ----------
            D("暂无扫描结果", "尚無掃描結果", "No results yet", "結果はまだありません");
            D("未发现重复文件 · 共检查 {0} 个文件 · 用时 {1:F1} 秒", "未發現重複檔案 · 共檢查 {0} 個檔案 · 耗時 {1:F1} 秒",
              "No duplicates found · {0} files checked · {1:F1}s", "重複なし · {0} 個のファイルを確認 · {1:F1} 秒");
            D("全选", "全選", "Check all", "すべて選択");
            D("全不选", "全不選", "Uncheck all", "すべて解除");
            D("排序", "排序", "Sort", "並べ替え");
            D("按大小 ↓（大→小）", "依大小 ↓（大→小）", "Size: large to small", "サイズ ↓（大→小）");
            D("按大小 ↑（小→大）", "依大小 ↑（小→大）", "Size: small to large", "サイズ ↑（小→大）");
            D("按时间 ↓（新→旧）", "依時間 ↓（新→舊）", "Date: newest first", "日付 ↓（新→旧）");
            D("按时间 ↑（旧→新）", "依時間 ↑（舊→新）", "Date: oldest first", "日付 ↑（旧→新）");
            D("按路径 A→Z", "依路徑 A→Z", "Path A to Z", "パス A→Z");
            D("删除", "刪除", "Delete", "削除");
            D("组", "組", "Group", "組");
            D("文件名", "檔名", "File name", "ファイル名");
            D("完整路径（双击行可打开文件）", "完整路徑（雙擊可開啟檔案）", "Full path (double-click to open)", "フルパス（ダブルクリックで開く）");
            D("大小", "大小", "Size", "サイズ");
            D("修改时间", "修改時間", "Modified", "更新日時");
            D("状态", "狀態", "Status", "状態");
            D("保留这份", "保留這份", "Keep", "この1件を残す");
            D("重复", "重複", "Duplicate", "重複");
            D("待删除", "待刪除", "To delete", "削除予定");
            D("打开文件", "開啟檔案", "Open file", "ファイルを開く");
            D("打开所在文件夹", "開啟所在資料夾", "Open containing folder", "保存先フォルダーを開く");
            D("复制完整路径", "複製完整路徑", "Copy full path", "フルパスをコピー");
            D("只保留这一份（同组其余勾选删除）", "只保留這一份（同組其餘勾選刪除）", "Keep only this one (check the rest for deletion)", "この1件だけ残す（他の複製を削除対象に）");
            D("取消本组全部勾选", "取消本組全部勾選", "Uncheck everything in this group", "この組のチェックをすべて解除");
            D("选中某一行，这里会显示该文件的完整路径（点击可复制）", "選取某一列，這裡會顯示該檔案的完整路徑（點擊可複製）",
              "Select a row to see its full path here (click to copy)", "行を選択すると、ここにフルパスが表示されます（クリックでコピー）");
            D("完整路径：{0}    （点击复制）", "完整路徑：{0}    （點擊複製）", "Full path: {0}    (click to copy)", "フルパス：{0}    （クリックでコピー）");
            D("每组保留", "每組保留", "Keep per group", "各組で残す1件");
            D("最早修改的那份", "最早修改的那份", "The oldest one", "最も古い1件");
            D("最新修改的那份", "最新修改的那份", "The newest one", "最も新しい1件");
            D("路径最短的那份", "路徑最短的那份", "The shortest path", "パスが最も短い1件");
            D("按规则勾选", "依規則勾選", "Auto-check", "ルールで自動選択");
            D("打开位置", "開啟位置", "Open location", "場所を開く");
            D("导出清单", "匯出清單", "Export list", "リスト出力");
            D("删除到回收站", "刪除到資源回收筒", "Delete (recycle)", "ごみ箱へ削除");
            D("永久删除", "永久刪除", "Delete (permanent)", "完全に削除");

            // ---------- 动态状态与提示 ----------
            D("选择要扫描的文件夹（可重复添加多个，也可以直接把文件夹拖进窗口）", "選擇要掃描的資料夾（可重複加入多個，也可直接拖進視窗）",
              "Choose folders to scan (add several, or just drag them into the window)", "スキャンするフォルダーを選択（複数追加、ウィンドウへのドラッグも可）");
            D("已添加 {0} 个扫描范围，点“开始扫描”。", "已加入 {0} 個掃描範圍，按「開始掃描」。",
              "{0} folder(s) added. Click Start scan.", "スキャン範囲を {0} 件追加しました。「スキャン開始」を押してください。");
            D("请先添加要扫描的文件夹。\n点“添加文件夹”，或直接把文件夹拖到窗口里。", "請先加入要掃描的資料夾。\n按「加入資料夾」，或直接把資料夾拖進視窗。",
              "Please add a folder to scan first.\nClick \"Add folder\" or drag a folder into the window.", "先にスキャンするフォルダーを追加してください。\n「フォルダー追加」を押すか、フォルダーをウィンドウへドラッグします。");
            D("还没有扫描范围", "尚未設定掃描範圍", "No folders added", "スキャン範囲がありません");
            D("准备扫描…", "準備掃描…", "Preparing...", "準備中…");
            D("已停止扫描。", "已停止掃描。", "Scan stopped.", "スキャンを停止しました。");
            D("扫描出错：{0}", "掃描錯誤：{0}", "Scan error: {0}", "スキャンエラー：{0}");
            D("扫描失败", "掃描失敗", "Scan failed", "スキャン失敗");
            D("扫描过程中出错：\n{0}", "掃描過程中發生錯誤：\n{0}", "An error occurred while scanning:\n{0}", "スキャン中にエラーが発生しました：\n{0}");
            D("扫描完成 · 检查 {0} 个文件 · 用时 {1:F1} 秒{2}{3}", "掃描完成 · 檢查 {0} 個檔案 · 耗時 {1:F1} 秒{2}{3}",
              "Done - {0} files checked in {1:F1} s{2}{3}", "完了 · {0} 件を検査 · {1:F1} 秒{2}{3}");
            D(" · {0} 个文件读取失败已跳过", " · {0} 個檔案讀取失敗已略過", " - {0} unreadable file(s) skipped", " · 読み取れない {0} 件をスキップ");
            D("没有发现重复文件。", "沒有發現重複檔案。", "No duplicate files found.", "重複ファイルは見つかりませんでした。");
            D("扫描完成", "掃描完成", "Scan finished", "スキャン完了");
            D("正在停止…", "正在停止…", "Stopping...", "停止中…");
            D("重复组 {0} · 文件 {1} · 可释放 {2} · 已勾 {3}", "重複組 {0} · 檔案 {1} · 可釋放 {2} · 已勾 {3}",
              "{0} groups · {1} files · {2} reclaimable · {3} checked", "{0} 組・{1} 件・{2} 解放可能・{3} 件選択");
            D("无法打开文件：\n{0}", "無法開啟檔案：\n{0}", "Could not open file:\n{0}", "ファイルを開けません：\n{0}");
            D("打开失败", "開啟失敗", "Open failed", "開けませんでした");
            D("无法打开文件夹：\n{0}", "無法開啟資料夾：\n{0}", "Could not open folder:\n{0}", "フォルダーを開けません：\n{0}");
            D("已复制 {0} 条路径到剪贴板。", "已複製 {0} 條路徑到剪貼簿。", "{0} path(s) copied to clipboard.", "{0} 件のパスをクリップボードにコピーしました。");
            D("还没有可清理的重复文件，请先扫描。", "尚無可清理的重複檔案，請先掃描。", "Nothing to clean yet - run a scan first.", "削除できる重複ファイルがありません。先にスキャンしてください。");
            D("提示", "提示", "Notice", "お知らせ");
            D("请先勾选要删除的文件。\n在“每组保留”里选一条规则即可自动勾选，每组留一份、其余勾上删除。",
              "請先勾選要刪除的檔案。\n在「每組保留」裡選一條規則即可自動勾選，每組留一份、其餘勾上刪除。",
              "Please check the files you want to delete.\nPick a rule in \"Keep per group\" and the checking is done for you: one kept per group, the rest checked.",
              "削除するファイルにチェックを入れてください。\n「グループごとに残す」でルールを選ぶと自動でチェックされます。");
            D("没有勾选文件", "未勾選檔案", "Nothing checked", "選択がありません");
            D("__del.perm", "永久刪除", "permanently delete", "完全に削除");
            D("__del.recycle", "移入資源回收筒", "move to the Recycle Bin", "ごみ箱へ移動");
            D("即将{0} {1} 个文件，释放约 {2}。\n\n{3}确定继续吗？", "即將{0} {1} 個檔案，釋放約 {2}。\n\n{3}確定要繼續嗎？",
              "About to {0} {1} file(s), freeing about {2}.\n\n{3}Continue?", "{0} {1} 件を実行し、約 {2} を解放します。\n\n{3}続行しますか？");
            D("注意：有 {0} 组的所有副本都被勾选，删除后这些内容将一份不剩。\n\n", "注意：有 {0} 組的所有副本都被勾選，刪除後這些內容將一份不剩。\n\n",
              "Note: every copy in {0} group(s) is checked - that content will be gone completely.\n\n", "注意：{0} 組はすべての複製が選択されており、実行すると完全に失われます。\n\n");
            D("永久删除无法恢复（不进回收站），确定继续吗？", "永久刪除無法復原（不進回收站），確定要繼續嗎？",
              "Permanent deletion cannot be undone (no Recycle Bin). Continue?", "完全削除は元に戻せません（ごみ箱にも入りません）。続行しますか？");
            D("删除后进入回收站，需要时可以还原。确定继续吗？", "刪除後進入回收站，需要時可以還原。確定要繼續嗎？",
              "Files go to the Recycle Bin and can be restored later. Continue?", "ごみ箱に入るので後から復元できます。続行しますか？");
            D("永久删除确认", "永久刪除確認", "Confirm permanent deletion", "完全削除の確認");
            D("清理确认", "清理確認", "Confirm cleanup", "整理の確認");
            D("正在永久删除…", "正在永久刪除…", "Deleting permanently...", "完全削除中…");
            D("正在移到回收站…", "正在移到資源回收筒…", "Moving to Recycle Bin...", "ごみ箱へ移動中…");
            D("清理完成，已{0} {1} 个文件、释放 {2}。{3}", "清理完成，已{0} {1} 個檔案、釋放 {2}。{3}",
              "Done - {0} {1} file(s), freed {2}. {3}", "完了 — {1} 件を{0}、{2} を解放。{3}");
            D("当前没有重复文件。", "目前已無重複檔案。", "No duplicates remain.", "重複ファイルは残っていません。");
            D("调用系统删除接口失败：", "呼叫系統刪除介面失敗：", "The system delete call failed: ", "システムの削除呼び出しに失敗：");
            D("{0} 个文件未能删除，常见原因：正被其他程序占用、没有权限、或位于不支持回收站的网络位置/移动磁盘。\n",
              "{0} 個檔案未能刪除，常見原因：正被其他程式占用、沒有權限，或位於不支援回收站的網路位置/行動磁碟。\n",
              "{0} file(s) could not be deleted - usually because they are in use, permission was denied, or they live on a network/removable drive without a Recycle Bin.\n",
              "{0} 件を削除できませんでした。原因として、使用中・権限なし・ごみ箱非対応のネットワーク/リムーバブルドライブが考えられます。\n");
            D("可以对这几个文件改用“永久删除”再试一次。", "可以對這幾個檔案改用「永久刪除」再試一次。",
              "You can try \"Delete permanently\" for these files.", "これらのファイルは「完全に削除」で再試行できます。");
            D("部分文件未删除", "部分檔案未刪除", "Some files not deleted", "一部のファイルを削除できませんでした");
            D("清理完成", "清理完成", "Cleanup finished", "整理完了");
            D("导出重复文件清单", "匯出重複檔案清單", "Export duplicate list", "重複ファイルリストの書き出し");
            D("CSV 文件 (*.csv)|*.csv", "CSV 檔案 (*.csv)|*.csv", "CSV files (*.csv)|*.csv", "CSV ファイル (*.csv)|*.csv");
            D("重复文件清单_{0}.csv", "重複檔案清單_{0}.csv", "duplicates_{0}.csv", "重複ファイル_{0}.csv");
            D("组号,文件名,完整路径,大小(字节),大小,修改时间,状态", "組號,檔名,完整路徑,大小(位元組),大小,修改時間,狀態",
              "Group,File name,Full path,Size (bytes),Size,Modified,Status", "組,ファイル名,フルパス,サイズ(バイト),サイズ,更新日時,状態");
            D("勾选删除", "勾選刪除", "Checked for delete", "削除対象");
            D("清单已导出：", "清單已匯出：", "List exported: ", "リストを書き出しました：");
            D("已导出到：\n{0}", "已匯出至：\n{0}", "Exported to:\n{0}", "書き出し先：\n{0}");
            D("导出完成", "匯出完成", "Export finished", "書き出し完了");
            D("导出失败：\n{0}", "匯出失敗：\n{0}", "Export failed:\n{0}", "書き出し失敗：\n{0}");
            D("导出失败", "匯出失敗", "Export failed", "書き出し失敗");
            D("还没有扫描结果可以导出。", "尚無掃描結果可匯出。", "No results to export yet.", "書き出せる結果がありません。");

            // ---------- 字体设置 ----------
            D("字体设置", "字型設定", "Font settings", "フォント設定");
            D("字体", "字型", "Font", "フォント");
            D("字号", "字級", "Size", "サイズ");
            D("预览", "預覽", "Preview", "プレビュー");
            D("确定", "確定", "OK", "OK");
            D("取消", "取消", "Cancel", "キャンセル");
            D("还原默认", "還原預設", "Reset", "初期化");
            D("预览文字：文件查重工具 Aa 123", "預覽文字：檔案查重工具 Aa 123", "Preview: Duplicate File Finder Aa 123", "プレビュー：重複ファイル検索 Aa 123");
            D("字号越大，界面整体越大（适合高分屏或视力不便时使用）。", "字級越大，介面整體越大（適合高解析螢幕或視力不便時使用）。",
              "A larger size scales the whole window - handy on high-DPI screens.", "サイズを大きくすると画面全体が大きくなります（高解像度画面に便利）。");

            // ---------- 自定义主题 ----------
            D("自定义主题", "自訂主題", "Custom theme", "カスタムテーマ");
            D("背景颜色", "背景顏色", "Background color", "背景色");
            D("背景图片", "背景圖片", "Background image", "背景画像");
            D("选择图片…", "選擇圖片…", "Choose image...", "画像を選択…");
            D("清除图片", "清除圖片", "Clear image", "画像をクリア");
            D("未选择", "未選擇", "None", "未選択");
            D("选择背景色", "選擇背景色", "Pick a color", "背景色を選択");
            D("卡片会自动保持可读的底色；图片会铺满窗口背景。", "卡片會自動保持可讀的底色；圖片會鋪滿視窗背景。",
              "Cards keep a readable background automatically; the image fills the window.", "カードは読みやすい下地を保ち、画像はウィンドウ全体に表示されます。");
            D("图片文件 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif", "圖片檔案 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
              "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif", "画像ファイル (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif");
            D("透明度", "透明度", "Transparency", "透明度");
            D("0% = 完全不透明，100% = 完全透明", "0% = 完全不透明，100% = 完全透明",
              "0% = fully opaque, 100% = fully transparent", "0% = 完全不透明、100% = 完全透明");

            // ---------- 关于 ----------
            D("当前版本", "目前版本", "Current version", "現在のバージョン");
            D("版本 1.0", "版本 1.0", "Version 1.0", "バージョン 1.0");
            D("版本 1.4", "版本 1.4", "Version 1.4", "バージョン 1.4");
            D("1.4 版：新增「选择助手」，按规则自动挑选保留的副本，可设每组留几份，并预览要删多少、能省多少空间；新增扫描完成统计报告；表格新增「创建时间」列，列可自由开关，列宽与窗口大小自动记忆。1.3 的透明度滑杆与深色提亮照旧。",
              "1.4 版：新增「選擇助手」，依規則自動挑選保留的副本，可設每組留幾份，並預覽要刪多少、能省多少空間；新增掃描完成統計報告；表格新增「建立時間」欄，欄位可自由開關，欄寬與視窗大小自動記憶。1.3 的透明度滑桿與深色提亮照舊。",
              "1.4: a Selection Assistant picks the copies to keep, previews how many files and how much space will go; a scan report after each scan; a Created column, toggleable columns, remembered widths and window size. The 1.3 transparency slider and brighter dark theme are unchanged.",
              "1.4：ルールで残すコピーを自動選択する「選択アシスタント」（削除件数と解放容量をプレビュー、グループごとの保持数も指定可）、スキャン完了レポート、作成日時列と列の表示切替、列幅・ウィンドウサイズの記憶を追加。1.3 の透明度スライダーとダークテーマの明るさはそのまま。");
            D("本工具为本地单机版，没有在线升级服务器；当前已是最新版本 1.4。",
              "本工具為本機單機版，沒有線上升級伺服器；目前已是最新版本 1.4。",
              "This is a standalone local tool with no online update server. You are on the latest version, 1.4.",
              "本ツールはローカル単体版で、オンライン更新サーバーはありません。現在の 1.4 が最新です。");
            D("开发者", "開發者", "Developer", "開発者");
            D("摄影师hujason", "攝影師hujason", "Photographer hujason", "フォトグラファー hujason");
            D("本版说明", "本版說明", "What's in this version", "このバージョンの内容");
            D("1.0 版：内容级查重（大小 → 取样指纹 → SHA-256）、按大小/时间/路径排序、回收站与永久删除、清单导出。",
              "1.0 版：內容級查重（大小 → 取樣指紋 → SHA-256）、依大小/時間/路徑排序、資源回收筒與永久刪除、清單匯出。",
              "1.0: content-level matching (size -> sampled fingerprint -> SHA-256), sorting by size/date/path, Recycle Bin or permanent delete, list export.",
              "1.0：内容レベル照合（サイズ → サンプル指紋 → SHA-256）、サイズ/日付/パスでの並べ替え、ごみ箱・完全削除、リスト出力。");
            D("升级说明", "升級說明", "About updates", "アップデートについて");
            D("本工具为本地单机版，没有在线升级服务器；当前已是最新版本 1.0。",
              "本工具為本機單機版，沒有線上升級伺服器；目前已是最新版本 1.0。",
              "This is a standalone local tool with no online update server. You are on the latest version, 1.0.",
              "本ツールはローカル単体版で、オンライン更新サーバーはありません。現在の 1.0 が最新です。");
            D("已是最新版本", "已是最新版本", "You are up to date", "最新版です");
            D("升级提示", "升級提示", "Update", "アップデート");
            D("作者信息", "作者資訊", "Author", "作者情報");
            D("关于作者信息", "關於作者資訊", "About the author", "作者について");
            D("关闭", "關閉", "Close", "閉じる");

            // ---------- 扫描进度 ----------
            D("正在遍历文件夹…", "正在走訪資料夾…", "Walking the folders...", "フォルダーを走査中…");
            D("共找到 {0} 个文件，正在按大小分组…", "共找到 {0} 個檔案，正在依大小分組…", "Found {0} files, grouping by size...", "{0} 件を検出、サイズでグループ化中…");
            D("大小相同的候选文件 {0} 个，正在比对内容指纹…", "大小相同的候選檔案 {0} 個，正在比對內容指紋…", "{0} candidate files share sizes, comparing fingerprints...", "同じサイズの候補 {0} 件、内容指紋を比較中…");
            D("正在比对内容指纹…", "正在比對內容指紋…", "Comparing content fingerprints...", "内容指紋を比較中…");
            D("正在精确校验：{0}", "正在精確校驗：{0}", "Verifying exactly: {0}", "厳密に検証中：{0}");
            D("正在遍历… 已查 {0} 个文件夹，找到 {1} 个文件", "正在走訪… 已查 {0} 個資料夾，找到 {1} 個檔案",
              "Scanning... {0} folders done, {1} files found", "走査中… {0} フォルダー確認、{1} 件を検出");

            // ---------- 删除结果 ----------
            D("已永久删除 {0} 个文件，释放约 {1} 空间。", "已永久刪除 {0} 個檔案，釋放約 {1} 空間。",
              "Permanently deleted {0} file(s), freed about {1}.", "{0} 件を完全削除し、約 {1} を解放しました。");
            D("已移入回收站 {0} 个文件，释放约 {1} 空间。", "已移入資源回收筒 {0} 個檔案，釋放約 {1} 空間。",
              "Moved {0} file(s) to the Recycle Bin, freed about {1}.", "{0} 件をごみ箱へ移動し、約 {1} を解放しました。");
            D("语言已切换，界面会在下次启动时保持。", "語言已切換，介面會在下次啟動時保持。",
              "Language switched. It will be remembered next time.", "言語を切り替えました。次回起動時も保持されます。");

            // ---------- 相似图片与预览 ----------
            D("相似图片（感知哈希，找“画面像”的照片）", "相似圖片（感知雜湊，找「畫面像」的照片）",
              "Similar photos (perceptual hash)", "類似写真（知覚ハッシュ）");
            D("阈值", "閾值", "Threshold", "しきい値");
            D("严格 ≤3", "嚴格 ≤3", "Strict ≤3", "厳格 ≤3");
            D("标准 ≤6", "標準 ≤6", "Normal ≤6", "標準 ≤6");
            D("宽松 ≤12", "寬鬆 ≤12", "Loose ≤12", "緩め ≤12");
            D("缩放、旋转、重压缩、转格式后仍能认出", "縮放、旋轉、重新壓縮、轉格式後仍能認出",
              "Still found after resize, rotate, re-compress or format change", "縮小・回転・再圧縮・形式変更後も検出");
            D("对比预览", "對比預覽", "Compare preview", "比較プレビュー");
            D("选中一组，这里并排显示缩略图和拍摄参数", "選取一組，這裡並排顯示縮圖與拍攝參數",
              "Select a group to see thumbnails and shooting data side by side", "グループを選ぶとサムネイルと撮影情報を並べて表示");
            D("在表格里选中一行，即可在这里预览并对照参数", "在表格中選取一列，即可在這裡預覽並對照參數",
              "Select a row in the table to preview and compare its details", "表の行を選ぶと、ここでプレビューと情報比較ができます");
            D("收起", "收合", "Collapse", "折りたたむ");
            D("展开", "展開", "Expand", "展開");
            D("半透明玻璃效果（卡片透出模糊的背景图）", "半透明玻璃效果（卡片透出模糊的背景圖）",
              "Translucent glass (cards show the blurred background)", "半透明ガラス（カードから背景が透ける）");
            D("全部结果", "全部結果", "All results", "すべての結果");
            D("仅精确重复", "僅精確重複", "Exact only", "完全一致のみ");
            D("仅相似图片", "僅相似圖片", "Similar only", "類似のみ");
            D("分辨率最高的那份", "解析度最高的那份", "Highest resolution", "最高解像度");
            D("参数", "參數", "Detail", "項目");
            D("文件", "檔案", "File", "ファイル");
            D("分辨率", "解析度", "Resolution", "解像度");
            D("位深 · 色域", "位元深度 · 色域", "Bit depth", "ビット深度");
            D("文件大小", "檔案大小", "File size", "ファイルサイズ");
            D("拍摄时间", "拍攝時間", "Taken", "撮影日時");
            D("相机", "相機", "Camera", "カメラ");
            D("镜头", "鏡頭", "Lens", "レンズ");
            D("修改时间", "修改時間", "Modified", "更新日時");
            D("画面相似度", "畫面相似度", "Similarity", "類似度");
            D("—", "—", "—", "—");
            D("相似 {0}%", "相似 {0}%", "{0}% similar", "類似 {0}%");
            D("保留 · 相似 {0}%", "保留 · 相似 {0}%", "Keep · {0}% similar", "保持・類似 {0}%");
            D("第 {0} 组 · 相似 {1}% · {2} 个文件", "第 {0} 組 · 相似 {1}% · {2} 個檔案",
              "Group {0} · {1}% similar · {2} files", "グループ {0}・類似 {1}%・{2} 件");
            D("第 {0} 组 · {1} 个相同文件", "第 {0} 組 · {1} 個相同檔案", "Group {0} · {1} identical files", "グループ {0}・同一ファイル {1} 件");
            D("当前筛选下没有结果，可切换“表格”查看其他结果", "目前篩選下沒有結果，可切換「表格」查看其他結果",
              "No results under this filter - switch the table filter to see others", "この絞り込みでは結果なし。「表」を切り替えてください");
            D(" · 相似组 {0}", " · 相似組 {0}", " · {0} similar groups", "・類似グループ {0}");
            D(" · 相似组 {0}（已算 {1} 张图片指纹）", " · 相似組 {0}（已算 {1} 張圖片指紋）",
              " - {0} similar group(s), {1} images hashed", "・類似グループ {0}（{1} 枚の指紋を計算）");
            D("正在计算图片指纹（共 {0} 张）…", "正在計算圖片指紋（共 {0} 張）…",
              "Hashing images (0 of {0})...", "画像の指紋を計算中（全 {0} 枚）…");
            D("正在计算图片指纹：{0}", "正在計算圖片指紋：{0}", "Hashing image: {0}", "画像の指紋を計算中：{0}");
            D("正在比对画面相似度（阈值 {0}/64）…", "正在比對畫面相似度（閾值 {0}/64）…",
              "Comparing visual similarity (threshold {0}/64)...", "画面の類似度を比較中（しきい値 {0}/64）…");
            D("正在比对画面相似度…", "正在比對畫面相似度…", "Comparing visual similarity...", "画面の類似度を比較中…");

            // ---------- 1.4 新增：创建时间 / 列显示 / 选择助手 / 扫描报告 ----------
            D("创建时间", "建立時間", "Created", "作成日時");
            D("列显示", "欄位顯示", "Columns", "列の表示");
            D("恢复默认列宽", "還原預設欄寬", "Reset column widths", "既定の列幅に戻す");
        D("恢复默认列序", "還原預設欄位順序", "Reset column order", "既定の列順に戻す");
        D("已恢复默认列序。", "已還原預設欄位順序。", "Column order reset.", "既定の列順に戻しました。");
            D("已恢复默认列宽。", "已還原預設欄寬。", "Column widths reset.", "既定の列幅に戻しました。");
            D("这一列是操作必需的，不能隐藏。", "這一欄是操作必需的，不能隱藏。",
              "This column is required and cannot be hidden.", "この列は操作に必要なので非表示にできません。");
            D("选择助手", "選擇助手", "Selection assistant", "選択アシスタント");
            D("选择助手…", "選擇助手…", "Selection assistant...", "選択アシスタント…");
            D("保留规则", "保留規則", "Keep rule", "保持ルール");
            D("每组保留份数", "每組保留份數", "Copies to keep per group", "グループごとに残す件数");
            D("文件名最短的那份", "檔名最短的那份", "Shortest file name", "ファイル名が最短のもの");
            D("应用范围", "套用範圍", "Apply to", "適用範囲");
            D("全部组", "全部組", "All groups", "すべてのグループ");
            D("应用勾选", "套用勾選", "Apply selection", "選択を適用");
            D("参与分组：{0} 组", "參與分組：{0} 組", "Groups involved: {0}", "対象グループ：{0}");
            D("保留 {0} 个文件，勾选删除 {1} 个", "保留 {0} 個檔案，勾選刪除 {1} 個",
              "Keep {0}, check {1} for deletion", "{0} 件を残し、{1} 件を削除対象に");
            D("预计释放约 {0}", "預計釋放約 {0}", "Frees about {0}", "約 {0} を解放");
            D("勾选后不会立刻删除，确认无误再点“删除到回收站”。", "勾選後不會立刻刪除，確認無誤再點「刪除到回收站」。",
              "Checking does not delete anything - review it, then click Delete (recycle).", "チェックだけでは削除されません。確認後に「ゴミ箱へ削除」を押してください。");
            D("选择助手已应用，结果已按规则勾选。", "選擇助手已套用，結果已按規則勾選。",
              "Assistant applied - rows checked by rule.", "アシスタントを適用し、ルールで選択しました。");
            D("还没有扫描结果，选择助手要在扫描之后用。", "還沒有掃描結果，選擇助手要在掃描之後用。",
              "No results yet - the assistant works after a scan.", "スキャン結果がありません。スキャン後に使えます。");
            D("扫描报告", "掃描報告", "Scan report", "スキャンレポート");
            D("扫描报告…", "掃描報告…", "Scan report...", "スキャンレポート…");
            D("扫描模式：{0}", "掃描模式：{0}", "Scan mode: {0}", "スキャンモード：{0}");
            D("共扫描 {0} 个文件（{1}）", "共掃描 {0} 個檔案（{1}）", "Scanned {0} files ({1})", "{0} 件をスキャン（{1}）");
            D("重复组：{0} 组（其中相似图片 {1} 组）", "重複組：{0} 組（其中相似圖片 {1} 組）",
              "Duplicate groups: {0} (similar photos: {1})", "重複グループ：{0}（類似写真 {1}）");
            D("重复文件：{0} 项，可释放 {1}", "重複檔案：{0} 項，可釋放 {1}",
              "Duplicate files: {0}, reclaimable {1}", "重複ファイル：{0} 件、解放可能 {1}");
            D("已计算图片指纹：{0} 张", "已計算圖片指紋：{0} 張", "Images hashed: {0}", "指紋を計算した画像：{0} 枚");
            D("读取失败跳过：{0} 项", "讀取失敗跳過：{0} 項", "Unreadable skipped: {0}", "読み取り失敗でスキップ：{0}");
            D("总耗时：{0:F1} 秒", "總耗時：{0:F1} 秒", "Elapsed: {0:F1} s", "合計時間：{0:F1} 秒");
            D("下面是本次扫描的统计，点“复制”可以贴到聊天或文档里。", "下面是本次掃描的統計，點「複製」可以貼到聊天或文件裡。",
              "Summary of the last scan - press Copy to paste it anywhere.", "今回のスキャン統計です。「コピー」で貼り付けできます。");
            D("以后每次扫描完成都自动显示这份统计", "以後每次掃描完成都自動顯示這份統計",
              "Show this summary automatically after every scan", "次回以降もスキャン後に自動表示");
            D("统计已复制到剪贴板。", "統計已複製到剪貼簿。", "Summary copied to clipboard.", "統計をクリップボードにコピーしました。");
            D("复制", "複製", "Copy", "コピー");
            D("还没有扫描结果，先扫描一次吧。", "還沒有掃描結果，先掃描一次吧。", "No scan results yet - run a scan first.", "スキャン結果がありません。まずスキャンしてください。");
            D("还没有扫描结果。", "還沒有掃描結果。", "No scan results yet.", "スキャン結果がありません。");
            D("扫描汇总", "掃描彙總", "Scan summary", "スキャン集計");
            D("扫描时间", "掃描時間", "Scan time", "スキャン日時");
            D("组号,文件名,完整路径,大小(字节),大小,修改时间,创建时间,状态", "組號,檔名,完整路徑,大小(位元組),大小,修改時間,建立時間,狀態",
              "Group,File name,Full path,Size (bytes),Size,Modified,Created,Status", "グループ,ファイル名,フルパス,サイズ(バイト),サイズ,更新日時,作成日時,状態");
            D("常规模式 · 相同内容(SHA-256)", "一般模式 · 相同內容(SHA-256)", "Normal mode · same content (SHA-256)", "通常モード・同一内容(SHA-256)");
            D("相似图片 + 内容查重", "相似圖片 + 內容查重", "Similar photos + content dedup", "類似写真＋内容の重複検出");
        }
    }
}



