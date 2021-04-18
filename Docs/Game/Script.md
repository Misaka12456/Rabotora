# Rabotora游戏脚本语法深度解析 / Rabotora Game Script Grammar Deep Resolution
本文档将讲解Rabotora游戏脚本(.rs(Rabotora Script))的文件结构及语法。  
This document will resolute Rabotora Game Script(.rs) file structure and grammar.

### 文件结构 / File Structure
一个标准的Rabotora游戏脚本文件应为如下格式:  
A standard Rabotora Game Script file should be the following format:

```php
# Rabotora Script Example
# 以下示例使用的游戏信息的url: http://unisonshift.amusecraft.com/products/project31/index.html
R.SetWindowSize(1280,720);
ResMgr.LoadRes(ResType.Font, 'fon', 'system/UDDigiKyokashoN-R.ttc', 2);
ResMgr.LoadRes(ResType.Image, 'splash', 'system/splash.png', 1);
var p_wait = R.NewWaitProperties();
p_wait.CanClickSkip = true;
var p_img = R.NewDrawingProperties();
p_img.FadeoutBeforeDrawing = true;
R.DrawImage('splash');
R.DrawString('This game is not suitable for the player who is under 18 years old.', 0.20, 0.10, 'fon', 0);
R.Wait(2500, p_wait);
R.DrawString('The work is copyrighted by UNISON Shift. Any infringement will be investigated.', 0.20, 0.10, 'fon', 0);
R.Wait(2500, p_wait);
R.DrawImage('@color/white', p_img);
ResMgr.LoadRes(ResType.Video, 'unisonshift', 'system/unisonshift.mp4', 1);
ResMgr.LoadRes(ResType.Audio, 'splashSound', 'sytem/splashSound.wav', 1);
R.PlayVideo('unisonshift');
R.PlaySound('splashSound');
R.Wait(500, p_wait);
R.GC();
R.DrawImage('@color/black', p_img);
ResMgr.LoadRes(ResType.Image, 'menu', 'system/menu.png');
ResMgr.LoadRes(ResType.Audio, 'menuBgm', 'system/bonbonScureSong.mp3');
R.Parallel(
    R.PlaySound('menuBgm', -1);
    R.DrawImage('menu');
    ResMgr.LoadRes(ResType.Audio, 'voice_1495', 'system/voice_1495.wav');
    ResMgr.LoadRes(ResType.Image, 'menuLeftLayer', 'system/menuLeftLayer.png');
);
R.Parallel(
    R.PlaySound('voice_1495');
    R.DrawImage('menuLeftLayer');
);
```

### 语法解析 / Grammar Resolution
Rabotora游戏脚本的基本语法类似PHP, 以下是解析:  
Rabotora Game Script Base Grammar is similar with PHP. Resolution is shown below.

#### 赋值语句 / Assignment Statement
```php
object = value;
```
或 / or
```php
var object = value;
```
其中``object``指要赋值的变量, ``value``是值。  
``object`` is the variable that you want to assign to and ``value`` is the value that will be assigned.  
Rabotora游戏脚本为弱类型脚本, 所有变量的类型在初次赋值时确定, 之后运行过程中不能更改。  
Rabotora Game Script is weakly-typed script, all types of variables are determined at the initial assignment, which cannot change later.

#### 声明语句 / Declaration Statement
```php
var object;
```
其中``object``指要声明的变量(必须以英文字母开头)。  
``object`` is the variable that you want to declare (the name must start with an english letter).  
若在声明时需要直接对变量赋值, 强烈建议直接使用``var object = value;``语句。  
If you want to assign the value at the time the variable declared, strongly recommend you to use ``var object = value;`` statement directly.

#### 判断语句 / Judgment Statement
```php
if object == value: next-statement;
```
其中``object``指要判断的变量, ``value`` 是要判断的值。  
``object`` is the variable that you want to judge and ``value`` is the value to judge.  
``next-statement``指判断成功执行的子语句(可嵌套判断)。  
``next-statement`` is the sub-statement that will be executed if judge succeed(Judge statement nestable).  
Rabotora所支持的运算符有: / These are the operators that Rabotora supported:  
``== != < > <= >=``

#### 注释格式 / Comment Format
单行注释 / Single-line Comment:  
```php
语句 / Statements... //注释内容(Comment)
```
多行注释 / Multi-line Comment:  
```php
# 注释内容第一部分(Comment Part 1)
# 注释内容第二部分(Comment Part 2)
...
```

### Rabotora 运行库概述 / Rabotora Runtime Library Introduction
游戏脚本在运行时可以使用已封装的Rabotora运行库。  
Game Script can use the encapsulated Rabotora Runtime Library when executing.  
以下是常用的Rabotora运行库。  
The following are commonly used Rabotora Runtime Libraries.  
注:``[]``指可选参数 / Caution: ``[]`` means optional argument(s)

#### R - Rabotora核心运行库对象 / Rabotora Core Runtime Library Object
提供的方法类型:游戏所必需的核心功能(如窗口绘制等)  
Type of provided methods: Core functions for games (e.g.:Window Drawing)  
提供的部分常用方法:  /  Part of common-used provided methods:  
```php
# 设置游戏窗口大小 / Set the size of game window
# 注意:该语句不与窗口自动全屏的机制冲突 / Caution: This statement is not conflict with the mechanism of Window Auto-Full Screen
# width=窗口宽度(单位:像素) / Window Width(px)
# height=窗口高度(单位:像素) / Window Height(px)
R.SetWindowSize(width, height);

# 绘制字符串 / Draw a string
# value=要绘制的字符串内容 / The content of the string that will be drawn
# left=字符串起始位置的X值(百分比) / The X value of start position of the string(Percentage)
# top=字符串起始位置的Y值(百分比) / The Y value of start position of the string(Percentage)
# font=要使用的字体家族资源的id / The id of using Font-Family Resource
# fontId=要使用的字体家族资源中的字体编号,默认为0)  / The font id of using Font-Family Resource (Default:0)
R.DrawString(value, left, top, font[, fontId]);


# 绘制静态图片 / Draw a static image
# imgId=要绘制的图片资源id(可为 '@color/颜色id' 格式的内嵌资源) / The id of Image Resource that will be drawn (can be the embedded resource of '@color/Color Code' format)
# prop=图片绘制操作的属性对象(可为null) / The property of the drawing operation (can be null)
# left=图片起始位置的X值(百分比) / The X value of start position of the image(Percentage)
# top=图片起始位置的Y值(百分比) / The Y value of start position of the image(Percentage)
# width=图片相较于原始大小的宽度(百分比) / The related width of the original image(Percentage)
# height=图片相较于原始大小的长度(百分比) / The related height of the original image(Percentage)
R.DrawImage(imgId[, prop, left, top[, width, height]]);


# 播放音频 / Play an audio
# 提示:本方法会返回一个对象, 停止播放时需传入该对象 / Tip: The method will return a object that will needs when stopping playing
# audioId=要播放的音频资源id / The id of Audio Resource that will be played
# count=循环播放次数(留空/值为0即只播放一次; 值为-1表示永久循环直到停止) / Loop play times (leave blank/the value '0' means only play once; '-1' means play in a permanent loop until it stopped)
R.PlaySound(audioId[, count]);


# 停止播放音频 / Stop playing an audio
# audioPlayObj=要停止的音频的播放对象 / The playing object of the stopping audio resource
R.StopSound(audioPlayObj);


# 播放动态图片 / Play a dynamic image
# 提示:本方法会返回一个对象, 停止播放时需传入该对象 / Tip: The method will return a object that will needs when stopping playing
# dyImgId=要播放的动态图片资源id / The id of Dynamic Image Resource that will be played
# count=循环播放次数(留空/值为0即只播放一次; 值为-1表示永久循环直到停止) / Loop play times (leave blank/the value '0' means only play once; '-1' means play in a permanent loop until it stopped)
# prop=图片绘制操作的属性对象(可为null) / The property of the drawing operation (can be null)
# left=图片起始位置的X值(百分比) / The X value of start position of the image(Percentage)
# top=图片起始位置的Y值(百分比) / The Y value of start position of the image(Percentage)
# width=图片相较于原始大小的宽度(百分比) / The related width of the original image(Percentage)
# height=图片相较于原始大小的长度(百分比) / The related height of the original image(Percentage)
R.PlayImage(dyImgId[, count[, prop, left, top, width, height]]);


# 停止播放动态图片 / Stop playing a dynamic image
# dyImgPlayObj=要停止的动态图片的播放对象 / The playing object of the stopping dynamic image resource
R.StopImage(dyImgPlayObj);

# 播放视频 / Play a video
# 提示:本方法会返回一个对象, 停止播放时需传入该对象 / Tip: The method will return a object that will needs when stopping playing
# 注意:视频将会在所有场景的最上层播放 / Caution: Video will be played over all scenes
# videoId=要播放的视频资源id / The id of Video Resource that will be played
# count=循环播放次数(留空/值为0即只播放一次; 值为-1表示永久循环直到停止) / Loop play times (leave blank/the value '0' means only play once; '-1' means play in a permanent loop until it stopped)
R.PlayVideo(videoId[, count]);


# 停止播放视频 / Stop playing a video
# videoPlayObj=要停止的视频的播放对象 / The playing object of the stopping video resource
R.StopVideo(videoPlayObj);

# 执行垃圾收集 / Run garbage collector
# 注意: 垃圾收集器会清理所有显式声明有限的使用次数且在已使用设定的次数后的资源。
# Caution: Garbage collector will dispose the resources that explicit declared using time and had been used the set times.
R.GC();

# 暂停运行一定时长 / Temporarily Sleep running
# time=暂停运行的时长(单位:毫秒) / The duration of sleep(ms)
# prop=暂停运行操作的属性 / The properties of sleep running operation
R.Wait(time[, prop]);

# 同时运行多个语句 / Execute several statements in the same time
# statements=要同时运行的语句集合(使用{}包围) / The statements that will be execute in the same time(surrounded by '{}')
R.Parallel(statements);
```

#### ResMgr - Rabotora资源管理器对象 / Rabotora Resource Manager Object
提供的方法类型:资源加载相关的功能  
Type of provided methods: Functions of resource loading  
提供的部分常用方法:  /  Part of common-used provided methods:  
```php
# 加载资源 / Load a resource
# type=资源的类型 / Resource Type
# id=加载完成后使用的资源id / Resource id that will be used after loading
# resPath=资源的路径(格式:'数据包名/资源文件的原始名(包括扩展名)') / Resource Path (Format: 'Data Package Name/The original name of the resource(including the extension filename)')
# usingTimes=资源的使用次数(值为'0'即使用一次;留空/值为'-1'即长期使用(通知垃圾收集器不收集该资源)) / Using times of the resource('0' means use once; '-1' or leave blank means long-time using(notify the garbage collector not to dispose this resource))
ResMgr.LoadRes(type, id, resPath[, usingTimes]);
```
使用的资源类型: / Resource type that being used:  
- ResType.Image: 静态图片资源 / Static Image Resources
- ResType.DyImage: 动态图片资源 / Dynamic Image Resources
- ResType.Audio: 音频资源 / Audio Resources
- ResType.Video: 视频资源 / Video Resources
- ResType.Font: 字体资源 / Font Resources
- ResType.Binary: 二进制资源 / Binary Resources
- ResType.Other: 其它类型资源 / Resources of other types

--------
最后编辑日期:2021.4.18  
Last Edited Date:4/18/2021