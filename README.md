# 文件查重工具（File Dedup Tool）

一个免安装的 Windows 单文件查重工具：双击 exe 就能用，不需要安装，也不需要额外运行时（Windows 自带的 .NET Framework 4.x 即可）。

按**文件内容**判断重复，不看文件名、不看后缀；支持相似照片识别，并把每个副本的完整路径列出来，按需删除到回收站或永久删除。

## 功能概览

- 三级内容查重：大小分桶 → 头 8K + 尾 4K 采样 MD5 → 全文 SHA-256
- 约 380 种扩展名识别（图片 / 视频 / 文档 / 音频 / 压缩包 / 其他），可自定义补充扩展名
- 「全文件扫描」忽略扩展名，按内容哈希比对，改名、改后缀也能查出来
- 「相似图片」：64 位感知哈希 + BK 树检索，缩放 / 旋转 / 重压缩 / 转格式后仍能认出，三档阈值（严格 ≤3 / 标准 ≤6 / 宽松 ≤12）
- 对比预览：一组内最多 4 张缩略图并排，对照分辨率、位深 · 色域、文件大小、拍摄时间、相机、镜头、画面相似度
- 跨目录、跨盘、不同路径的查重天然成立（全局大小桶 + 全局哈希）
- 每组保留规则：最早修改 / 最新修改 / 最短路径 / 分辨率最高
- 回收站删除、永久删除、只读文件处理、CSV 导出、暂停 / 继续 / 取消
- 四套界面皮肤（简约模式＝1.4 原界面 / 白色主题（默认，青灰配色 + 渐变横幅 + 圆角描边卡片）/ 深色主题 / 自定义背景色或背景图 + 毛玻璃半透明 / 透明度滑杆），六语言（简 / 繁 / 英 / 日 / 俄 / 韩），字体缩放
- 窗口默认按设计尺寸居中打开、整窗保证落在屏幕可用区域内（小分辨率不铺满），切换皮肤不动窗口大小与位置

## 编译

需要 .NET Framework 4.x（Windows 10 / 11 自带），**不需要 Visual Studio**。

```powershell
cd 源码
pwsh -File build.ps1
```

脚本会调用系统里的 `csc.exe`（`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`）编译 `src\*.cs`，输出 `文件查重工具.exe`。

注意：

- 源码按 **C# 5** 写（`csc.exe` 的语法上限），不能用字符串插值 `$"..."`、`?.`、元组等 C# 6+ 语法
- 编译前脚本会结束正在运行的旧 exe，否则会报文件占用 / CS0016
- 想用 Visual Studio 打开：新建「Windows 窗体应用(.NET Framework)」项目，把 `src` 里的 `.cs`、`app.ico`、`app.manifest` 加进去，删掉自带的 `Form1`，入口是 `Program.cs`，窗体是 `MainForm`

## 源码结构

| 文件 | 作用 |
| --- | --- |
| `Program.cs` | 程序入口，启动 `MainForm` |
| `MainForm.cs` | 主窗口：界面搭建、扫描流程、结果表格、对比预览、删除操作 |
| `Core.cs` | 查重核心：目录遍历、大小分桶、采样哈希、全文哈希、结果分组 |
| `ImageInfo.cs` | 图片信息与相似度：EXIF 读取、64 位感知哈希、BK 树索引、RAW 内嵌预览提取 |
| `Theme.cs` | 配色与主题（简约 / 白色 / 深色 / 自定义）、标题栏深色、毛玻璃开关 |
| `Dialogs.cs` | 各种对话框：自定义主题、字体、关于软件、关于作者 |
| `Lang.cs` | 简体 / 繁体 / 英文 / 日文词条表（键是简体原文） |
| `LangRuKo.cs` | 俄语 / 韩语词条表（同样以简体原文为键，未命中时回退英文） |
| `Settings.cs` | 读写 `%LOCALAPPDATA%\FileDedupTool\settings.ini` |
| `UiKit.cs` | 自绘控件：进度条、禁用态按钮 |
| `ComboButton.cs` | 自绘下拉选择控件 |
| `MenuStyle.cs` | 菜单渲染风格 |
| `ShellDelete.cs` | 调用 Shell API 删除到回收站 |
| `Version.cs` | 程序集版本号 |
| `app.manifest` | 权限、DPI 感知、长路径支持 |
| `app.ico` | 程序图标 |

## 配置

配置存在 `%LOCALAPPDATA%\FileDedupTool\settings.ini`，键含义：

| 键 | 说明 |
| --- | --- |
| `theme` | `light` / `dark` / `custom` |
| `bgColor` | 自定义背景色，`R,G,B` |
| `bgImage` | 自定义背景图路径 |
| `glass` | 毛玻璃开关 `1/0` |
| `glassOpacity` | 毛玻璃透明度 `0`（不透明）–`100`（全透明） |
| `lang` | `zh-CN` / `zh-TW` / `en` / `ja` / `ru` / `ko` |
| `fontName` / `fontPt` | 字体与字号 |
| `scanDepth` | 扫描层级 `0`–`4`（`4` = 不限层级） |
| `scanAll` | 全文件扫描 `1/0` |
| `scanSimilar` | 相似图片 `1/0` |
| `similarThreshold` | 相似阈值 `3` / `6` / `12` |

想恢复初始状态，删掉这个 ini 即可。

## 改代码的常用入口

- 改文案：`Lang.cs`（简 / 繁 / 英 / 日）、`LangRuKo.cs`（俄 / 韩）；键是简体原文，改键要同步改另一处
- 加新语种：在 `Lang.cs` 加常量、`MainForm.cs` 语言菜单加一项、`Settings.cs` 的 lang 白名单加代码，再照 `LangRuKo.cs` 的 `D("简体原文", "译文", ...)` 格式补一张词条表
- 改配色：`Theme.cs` 的 `Refresh()`（简约 / 浅色 / 深色三套，`Simple = 3` 是简约模式）
- 改界面布局：`MainForm.cs` 的 `BuildUi()` 与 `RebuildUi()`（`Theme.IsSimple()` 分支保留 1.4 原界面）
- 改查重逻辑：`Core.cs`
- 改相似算法：`ImageInfo.cs`（感知哈希维度、阈值策略）
- 改版本号：`MainForm.AppVersion`、`Version.cs`、`Lang.cs` 里的「版本 x.y」与版本说明

## 注意

这个工具会**真实删除文件**（回收站 / 永久删除都是真删）。改代码或测试时，请先拿一份复制出来的测试目录练手，别对着唯一一份素材跑。

## 许可

MIT License，见 `LICENSE`。如果要开源，建议保留作者署名（摄影师hujason）与各平台主页链接。
