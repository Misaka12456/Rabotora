# Project Rabotora

下一代视觉小说(VN)开源游戏引擎  
The Next Generation's Open-Source Visual Novel Game Engine  
[![Join the Official QQ Group - Rabotora Tower](https://img.shields.io/badge/chat-on%20QQ-blue)](https://jq.qq.com/?_wv=_1027&k=PizxWl18) [![Misaka Castle Member Project](https://img.shields.io/badge/Misaka%20Castle-Member%20Project-fuchsia)](https://misakacastle.moe) ![123 Open-Source Organization 10 Years Appointment Member Project](https://img.shields.io/badge/Team123it%2010%20Years%20Appointment-Member%20Project-brightgreen) [![LICENSE](https://img.shields.io/badge/license-Team123it--MIT%202.0-blue)](https://github.com/Misaka12456/Rabotora/blob/master/LICENSE)

## 解决方案结构 / Solution Structure

- Rabotora.Core: Rabotora 核心库 / Rabotora Core Library
- Rabotora.Launcher: Rabotora 游戏启动器 / Rabotora Game Launcher

## 开发环境依赖 / Developing Environment Dependencies

- JetBrains Rider 2023.3+ / Microsoft Visual Studio 2022+ 或其它所有支持.NET 8.0+的IDE  
  JetBrains Rider 2023.3+ / Microsoft Visual Studio 2022+ or other IDEs that support .NET 8.0+
- Microsoft.NetCore.App (v8.0.0+)
- Microsoft.WindowsDesktop.App (v8.0.0+)

## 工作原理 / Operating Principle

将游戏数据打包为多个pack(运行时实时读取);  
将使用[Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows "Vortice.Windows")开发的游戏主程序按照每个Rabotora游戏项目的设置加载并读取入口点脚本。

Pack the data needed for the game into multiple files(will be read in real-time at run time).  
Load the Game Launcher which is developed by [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows "Vortice.Windows") according to the settings of every Rabotora Game Project(read the entry Rabotora Script).

## 一起聊有关Rabotora的话题吧 / Chat some topics associated with Rabotora

QQ官方讨论群 / Official QQ Discussing Group : [653640137](https://jq.qq.com/?_wv=_1027&k=PizxWl18)  
Discord: [https://discord.gg/2Mt3NVFFUk](https://discord.gg/2Mt3NVFFUk)

## 开源协议 / Open-Source License

根据[123 Open-Source Organization MIT Public License v2.0](LICENSE)授权  
Licensed under [123 Open-Source Organization MIT Public License v2.0](LICENSE).

---

最后更新日期: 2025.4.9  
Last Update: April 9th, 2022
