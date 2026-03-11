<div align="center">

# RabotoraX

---
跨平台、完全开源的2D/3D游戏引擎。  
Cross-platform, fully open-source 2D/3D game engine.

[![Join the Official QQ Group - Rabotora Tower](https://img.shields.io/badge/chat-on%20QQ-blue)](https://jq.qq.com/?_wv=_1027&k=PizxWl18) [![Misaka Castle Member Project](https://img.shields.io/badge/Misaka%20Castle-Member%20Project-fuchsia)](https://misakacastle.moe) ![123 Open-Source Organization 10 Years Appointment Member Project](https://img.shields.io/badge/Team123it%2010%20Years%20Appointment-Member%20Project-brightgreen) [![LICENSE](https://img.shields.io/badge/license-Team123it--MIT%202.0-blue)](https://github.com/MisakaRehana/Rabotora/blob/master/LICENSE)

</div>

**<div style="color:orange">注意：RabotoraX目前仍处于早期开发阶段，功能尚不完整，可能存在不稳定因素。请谨慎在生产环境使用，并欢迎提交反馈和贡献代码！<br />
Note: RabotoraX is currently in early development stage, features are incomplete and may be unstable. Please use with caution in production and feel free to submit feedback and contribute code!</div>**

## 这是什么？ / What's this?
RabotoraX是一个重构自原始[Rabotora](https://github.com/MisakaRehana/Rabotora/tree/legacy-dev)项目的，基于.NET 10和C#的2D/3D游戏引擎。  
架构支持不同图形API后端（如DirectX、OpenGL等），旨在为开发者提供一个现代化、易用且高性能的游戏开发平台。

RabotoraX is a 2D/3D game engine built on .NET 10 and C#, refactored from the original [Rabotora](https://github.com/MisakaRehana/Rabotora/tree/legacy-dev) project.  
It features a modular architecture with support for multiple graphics API backends (like DirectX, OpenGL, etc.) and aims to provide developers with a modern, user-friendly, and high-performance platform for game development.

## 项目结构 / Project Structure
- [RabotoraX.Core](./RabotoraX.Core): 引擎核心库，包含渲染、物理、输入等基础功能 / Core library containing rendering, physics, input and other fundamental features.
- [RabotoraX.Interop.Direct3D11](./RabotoraX.Interop.Direct3D11): DirectX 11 图形 API 后端实现 / DirectX 11 graphics API backend implementation.
- [RabotoraX.Interop.Win32](./RabotoraX.Interop.Win32): Windows 平台原生 API 封装 / Windows platform native API wrapper.
- [RabotoraX.Windows.Test](./RabotoraX.Windows.Test): Windows 平台示例项目 / Windows platform sample project.
- [RabotoraX.Analyzers](./RabotoraX.Analyzers): RabotoraX 的 Roslyn 代码分析器 / Roslyn code analyzers for RabotoraX.
- [RabotoraX.CodeFix](./RabotoraX.CodeFix): RabotoraX 的 Roslyn 代码修复器 / Roslyn code fix providers for RabotoraX.
- *更多平台和功能模块正在开发中... / More platforms and feature modules are under development...*

## 图形后端开发路线图 / Graphics Backends Roadmap
- [x] **DirectX 11** (Windows) [开发中 / In Development]
- [ ] **OpenGL** (跨平台 / Cross-platform)
- [ ] **OpenGL ES 3** (Android)
- [ ] **Vulkan** (跨平台 / Cross-platform)

## 开源协议 / Open-Source License

根据[123 Open-Source Organization MIT Public License v2.0](LICENSE)授权  
Licensed under [123 Open-Source Organization MIT Public License v2.0](LICENSE).

---

最后更新日期: 2026.3.12
Last Update: Mar 12th, 2026