# Project Rabotora

下一代视觉小说(VN)开源游戏引擎  
The Next Generation's Open-Source Visual Novel Game Engine  
[点击这里查看版权公告 / Click here to jump to Copyright Announcement](Miscellaneous/cprt-anno.md)  
[![Join the chat at https://gitter.im/Rabotora/community](https://badges.gitter.im/Rabotora/community.svg)](https://gitter.im/Rabotora/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Join the Official QQ Group - Rabotora Tower](https://img.shields.io/badge/chat-on%20QQ-blue)](https://jq.qq.com/?_wv=_1027&k=PizxWl18) [![Misaka Castle Member Project](https://img.shields.io/badge/Misaka%20Castle-Member%20Project-fuchsia)](https://misakacastle.moe) ![123 Open-Source Organization 10 Years Appointment Member Project](https://img.shields.io/badge/Team123it%2010%20Years%20Appointment-Member%20Project-brightgreen) [![LICENSE](https://img.shields.io/badge/license-Team123it--MIT%202.0-blue)](https://github.com/Misaka12456/Rabotora/blob/master/LICENSE)

## 解决方案结构 / Solution Structure

- Rabotora.Core: Rabotora 核心库 / Rabotora Core Library
- Rabotora.Launcher: Rabotora 游戏启动器 / Rabotora Game Launcher
- Rabotora.Runtime: Rabotora 运行时环境库(提供大量API以对接Direct2D原生API) / Rabotora Runtime Environment Library (Presents a large amout of APIs for docking Direct2D API)

## 开发环境依赖 / Developing Environment Dependencies

- Microsoft Visual Studio 2022 (v17.0+) 64位/64 Bit
- Microsoft.NetCore.App (v6.0.0+)
- Microsoft.WindowsDesktop.App (v6.0.0+)
- ANTLR (v4.6.6+)

## 工作原理 / Operating Principle

基于[ANTLR](https://github.com/antlr/antlr4 "ANTLR4")语法/词法树分析库实现:  
将游戏数据打包为多个pack(运行时实时读取);  
将使用[SharpDX](https://github.com/sharpdx/SharpDX "SharpDX")开发的游戏主程序按照每个Rabotora游戏项目的设置加载并读取入口点脚本。

Implemented based on [ANTLR](https://github.com/antlr/antlr4 "ANTLR4") Library:  
Pack the data needed for the game into multiple files(will be read in real-time at run time).  
Load the Game Launcher which is developed by [SharpDX](https://github.com/sharpdx/SharpDX "SharpDX") according to the settings of every Rabotora Game Project(read the entry Rabotora Script).

## 一起聊有关Rabotora的话题吧 / Chat some topics associated with Rabotora

QQ官方讨论群 / Official QQ Discussing Group : [653640137](https://jq.qq.com/?_wv=_1027&k=PizxWl18)  
Gitter: [https://gitter.im/Rabotora/community](https://gitter.im/Rabotora/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)  
Discord: [https://discord.gg/2Mt3NVFFUk](https://discord.gg/2Mt3NVFFUk)

## 开源协议 / Open-Source License

根据[123 Open-Source Organization MIT Public License v2.0](LICENSE)授权  
Licensed under [123 Open-Source Organization MIT Public License v2.0](LICENSE).

---

最后更新日期: 2022.5.9  
Last Update: 5/9/2022
