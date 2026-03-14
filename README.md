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

## 构建须知 / Build Notes
1. 核心库RabotoraX.Core依赖于Android SDK以完成针对``net*-android``目标框架的构建。请确保已安装Android SDK并正确配置``ANDROID_SDK_ROOT``环境变量(参阅[RabotoraX.Core / Directory.Build.props](./RabotoraX.Core/Directory.Build.props)中的相关注释)。  
   The core library RabotoraX.Core relies on the Android SDK to build for ``net*-android`` target frameworks. Please ensure you have the Android SDK installed and the ``ANDROID_SDK_ROOT`` environment variable properly set (see related comments in [RabotoraX.Core / Directory.Build.props](./RabotoraX.Core/Directory.Build.props)).
## 开源协议 / Open-Source License

根据[123 Open-Source Organization MIT Public License v2.0](LICENSE)授权发布。  
一些第三方库和资源可能使用不同的许可证，请参阅各自的文档以及[许可附录](LICENSE-Appendix.txt)以获取详细信息。  
Licensed under [123 Open-Source Organization MIT Public License v2.0](LICENSE).   
Some third-party libraries and assets may be under different licenses, please refer to their respective documentation and the [License Appendix](LICENSE-Appendix.txt) for details.

---

最后更新日期: 2026.3.15  
Last Update: Mar 15th, 2026