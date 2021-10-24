<b>Update: DO NOT DOWNLOAD RABOTORA PROJECT SOURCE CODE FROM THIRD-PARTY SITE INSTEAD OF GITHUB! <br/>
更新：不要在除Github外的任何第三方网站下载Project Rabotora的源代码！</b> <br/>
[点击这里查看公告 / Click here to jump to Announcement](https://github.com/Misaka12456/Rabotora/blob/master/Docs/cprt-anno.md) 
-----------
# Rabotora
下一代视觉小说(VN)开源游戏引擎  
The Next Generation's Open-Source Visual Novel Game Engine  
[![Join the chat at https://gitter.im/Rabotora/community](https://badges.gitter.im/Rabotora/community.svg)](https://gitter.im/Rabotora/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Misaka Castle Member Project](https://img.shields.io/badge/Misaka%20Castle-member%20project-fuchsia)](https://misakacastle.moe)
[![Rabotora](https://img.shields.io/github/license/Misaka12456/Rabotora)](https://github.com/Misaka12456/Rabotora/blob/master/LICENSE)

## 开发文档 / Development Documents
[点击这里跳转到Rabotora引擎的游戏开发文档 / Click here to jump to Rabotora Game Developing Documents](https://github.com/Misaka12456/Rabotora/blob/master/Docs)

## 解决方案结构 / Solution Structure
- Rabotora.CLI: Rabotora Host (提供CLI核心功能 / Providing CLI Core Functions)
- Rabotora.Compiler: Rabotora 游戏编译器 / Rabotora Game Compiler
- Rabotora.Core: Rabotora 核心库 / Rabotora Core Library
- Rabotora.Creator: Rabotora 游戏开发集成环境 / Rabotora Game Developing IDE
- Rabotora.Launcher: Rabotora 游戏启动器 / Rabotora Game Launcher (会在游戏程序生成时重编译 / will be re-compiled when building a new game)
- Rabotora.MultiLanguage: Rabotora 系统语言包支持 / Rabotora System Multi-Language Packages Support
- Rabotora.Lite: Rabotora 轻量版 / Rabotora Lite (可嵌入至其它引擎开发的游戏中 / Can be embedded in the game which is built by another game engine)

## 开发环境依赖 / Developing Environment Dependencies
- Microsoft Visual Studio 2019 (v16.8+) 64位/64 Bit
- .NET SDK (Microsoft.NetCore.App) (v5.0.0+)
- .NET Framework (v4.7.2+)

## 工作原理 / Operating Principle
基于.NET动态编译机制和<a href="https://github.com/dotnet/roslyn" title="Roslyn">开源编译引擎("Roslyn")</a>实现:  
将游戏数据打包为多个pack(运行时实时读取);  
将游戏主程序按照每个Rabotora游戏项目的设置重编译(同时嵌入运行库(.NET))。

Implemented based on .NET Dynamic Compilation Mechanism and <a href="https://github.com/dotnet/roslyn" title="Roslyn">Open-Source Compilation Engine ("Roslyn")</a>:  
Pack the data needed for the game into multiple files(will be read in real-time at run time).  
Re-Compile the Game Launcher according to the settings of every Rabotora Game Project(embed .NET runtime at the same time).

## 开源协议 / Open-Source License
根据<a href="https://github.com/Misaka12456/Rabotora/blob/master/LICENSE" title="MIT开源许可协议">MIT开源许可协议</a>授权  
Licensed under <a href="https://github.com/Misaka12456/Rabotora/blob/master/LICENSE" title="The MIT License">The MIT License</a>.

-----
最后更新日期: 2021.10.25
Last Update: October 25, 2021