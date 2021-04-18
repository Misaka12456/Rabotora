# Rabotora��Ϸ�ű��﷨��Ƚ��� / Rabotora Game Script Grammar Deep Resolution
���ĵ�������Rabotora��Ϸ�ű�(.rs(Rabotora Script))���ļ��ṹ���﷨��  
This document will resolute Rabotora Game Script(.rs) file structure and grammar.

### �ļ��ṹ / File Structure
һ����׼��Rabotora��Ϸ�ű��ļ�ӦΪ���¸�ʽ:  
A standard Rabotora Game Script file should be the following format:

```php
# Rabotora Script Example
# ����ʾ��ʹ�õ���Ϸ��Ϣ��url: http://unisonshift.amusecraft.com/products/project31/index.html
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

### �﷨���� / Grammar Resolution
Rabotora��Ϸ�ű��Ļ����﷨����PHP, �����ǽ���:  
Rabotora Game Script Base Grammar is similar with PHP. Resolution is shown below.

#### ��ֵ��� / Assignment Statement
```php
object = value;
```
�� / or
```php
var object = value;
```
����``object``ָҪ��ֵ�ı���, ``value``��ֵ��  
``object`` is the variable that you want to assign to and ``value`` is the value that will be assigned.  
Rabotora��Ϸ�ű�Ϊ�����ͽű�, ���б����������ڳ��θ�ֵʱȷ��, ֮�����й����в��ܸ��ġ�  
Rabotora Game Script is weakly-typed script, all types of variables are determined at the initial assignment, which cannot change later.

#### ������� / Declaration Statement
```php
var object;
```
����``object``ָҪ�����ı���(������Ӣ����ĸ��ͷ)��  
``object`` is the variable that you want to declare (the name must start with an english letter).  
��������ʱ��Ҫֱ�ӶԱ�����ֵ, ǿ�ҽ���ֱ��ʹ��``var object = value;``��䡣  
If you want to assign the value at the time the variable declared, strongly recommend you to use ``var object = value;`` statement directly.

#### �ж���� / Judgment Statement
```php
if object == value: next-statement;
```
����``object``ָҪ�жϵı���, ``value`` ��Ҫ�жϵ�ֵ��  
``object`` is the variable that you want to judge and ``value`` is the value to judge.  
``next-statement``ָ�жϳɹ�ִ�е������(��Ƕ���ж�)��  
``next-statement`` is the sub-statement that will be executed if judge succeed(Judge statement nestable).  
Rabotora��֧�ֵ��������: / These are the operators that Rabotora supported:  
``== != < > <= >=``

#### ע�͸�ʽ / Comment Format
����ע�� / Single-line Comment:  
```php
��� / Statements... //ע������(Comment)
```
����ע�� / Multi-line Comment:  
```php
# ע�����ݵ�һ����(Comment Part 1)
# ע�����ݵڶ�����(Comment Part 2)
...
```

### Rabotora ���п���� / Rabotora Runtime Library Introduction
��Ϸ�ű�������ʱ����ʹ���ѷ�װ��Rabotora���п⡣  
Game Script can use the encapsulated Rabotora Runtime Library when executing.  
�����ǳ��õ�Rabotora���п⡣  
The following are commonly used Rabotora Runtime Libraries.  
ע:``[]``ָ��ѡ���� / Caution: ``[]`` means optional argument(s)

#### R - Rabotora�������п���� / Rabotora Core Runtime Library Object
�ṩ�ķ�������:��Ϸ������ĺ��Ĺ���(�細�ڻ��Ƶ�)  
Type of provided methods: Core functions for games (e.g.:Window Drawing)  
�ṩ�Ĳ��ֳ��÷���:  /  Part of common-used provided methods:  
```php
# ������Ϸ���ڴ�С / Set the size of game window
# ע��:����䲻�봰���Զ�ȫ���Ļ��Ƴ�ͻ / Caution: This statement is not conflict with the mechanism of Window Auto-Full Screen
# width=���ڿ��(��λ:����) / Window Width(px)
# height=���ڸ߶�(��λ:����) / Window Height(px)
R.SetWindowSize(width, height);

# �����ַ��� / Draw a string
# value=Ҫ���Ƶ��ַ������� / The content of the string that will be drawn
# left=�ַ�����ʼλ�õ�Xֵ(�ٷֱ�) / The X value of start position of the string(Percentage)
# top=�ַ�����ʼλ�õ�Yֵ(�ٷֱ�) / The Y value of start position of the string(Percentage)
# font=Ҫʹ�õ����������Դ��id / The id of using Font-Family Resource
# fontId=Ҫʹ�õ����������Դ�е�������,Ĭ��Ϊ0)  / The font id of using Font-Family Resource (Default:0)
R.DrawString(value, left, top, font[, fontId]);


# ���ƾ�̬ͼƬ / Draw a static image
# imgId=Ҫ���Ƶ�ͼƬ��Դid(��Ϊ '@color/��ɫid' ��ʽ����Ƕ��Դ) / The id of Image Resource that will be drawn (can be the embedded resource of '@color/Color Code' format)
# prop=ͼƬ���Ʋ��������Զ���(��Ϊnull) / The property of the drawing operation (can be null)
# left=ͼƬ��ʼλ�õ�Xֵ(�ٷֱ�) / The X value of start position of the image(Percentage)
# top=ͼƬ��ʼλ�õ�Yֵ(�ٷֱ�) / The Y value of start position of the image(Percentage)
# width=ͼƬ�����ԭʼ��С�Ŀ��(�ٷֱ�) / The related width of the original image(Percentage)
# height=ͼƬ�����ԭʼ��С�ĳ���(�ٷֱ�) / The related height of the original image(Percentage)
R.DrawImage(imgId[, prop, left, top[, width, height]]);


# ������Ƶ / Play an audio
# ��ʾ:�������᷵��һ������, ֹͣ����ʱ�贫��ö��� / Tip: The method will return a object that will needs when stopping playing
# audioId=Ҫ���ŵ���Ƶ��Դid / The id of Audio Resource that will be played
# count=ѭ�����Ŵ���(����/ֵΪ0��ֻ����һ��; ֵΪ-1��ʾ����ѭ��ֱ��ֹͣ) / Loop play times (leave blank/the value '0' means only play once; '-1' means play in a permanent loop until it stopped)
R.PlaySound(audioId[, count]);


# ֹͣ������Ƶ / Stop playing an audio
# audioPlayObj=Ҫֹͣ����Ƶ�Ĳ��Ŷ��� / The playing object of the stopping audio resource
R.StopSound(audioPlayObj);


# ���Ŷ�̬ͼƬ / Play a dynamic image
# ��ʾ:�������᷵��һ������, ֹͣ����ʱ�贫��ö��� / Tip: The method will return a object that will needs when stopping playing
# dyImgId=Ҫ���ŵĶ�̬ͼƬ��Դid / The id of Dynamic Image Resource that will be played
# count=ѭ�����Ŵ���(����/ֵΪ0��ֻ����һ��; ֵΪ-1��ʾ����ѭ��ֱ��ֹͣ) / Loop play times (leave blank/the value '0' means only play once; '-1' means play in a permanent loop until it stopped)
# prop=ͼƬ���Ʋ��������Զ���(��Ϊnull) / The property of the drawing operation (can be null)
# left=ͼƬ��ʼλ�õ�Xֵ(�ٷֱ�) / The X value of start position of the image(Percentage)
# top=ͼƬ��ʼλ�õ�Yֵ(�ٷֱ�) / The Y value of start position of the image(Percentage)
# width=ͼƬ�����ԭʼ��С�Ŀ��(�ٷֱ�) / The related width of the original image(Percentage)
# height=ͼƬ�����ԭʼ��С�ĳ���(�ٷֱ�) / The related height of the original image(Percentage)
R.PlayImage(dyImgId[, count[, prop, left, top, width, height]]);


# ֹͣ���Ŷ�̬ͼƬ / Stop playing a dynamic image
# dyImgPlayObj=Ҫֹͣ�Ķ�̬ͼƬ�Ĳ��Ŷ��� / The playing object of the stopping dynamic image resource
R.StopImage(dyImgPlayObj);

# ������Ƶ / Play a video
# ��ʾ:�������᷵��һ������, ֹͣ����ʱ�贫��ö��� / Tip: The method will return a object that will needs when stopping playing
# ע��:��Ƶ���������г��������ϲ㲥�� / Caution: Video will be played over all scenes
# videoId=Ҫ���ŵ���Ƶ��Դid / The id of Video Resource that will be played
# count=ѭ�����Ŵ���(����/ֵΪ0��ֻ����һ��; ֵΪ-1��ʾ����ѭ��ֱ��ֹͣ) / Loop play times (leave blank/the value '0' means only play once; '-1' means play in a permanent loop until it stopped)
R.PlayVideo(videoId[, count]);


# ֹͣ������Ƶ / Stop playing a video
# videoPlayObj=Ҫֹͣ����Ƶ�Ĳ��Ŷ��� / The playing object of the stopping video resource
R.StopVideo(videoPlayObj);

# ִ�������ռ� / Run garbage collector
# ע��: �����ռ���������������ʽ�������޵�ʹ�ô���������ʹ���趨�Ĵ��������Դ��
# Caution: Garbage collector will dispose the resources that explicit declared using time and had been used the set times.
R.GC();

# ��ͣ����һ��ʱ�� / Temporarily Sleep running
# time=��ͣ���е�ʱ��(��λ:����) / The duration of sleep(ms)
# prop=��ͣ���в��������� / The properties of sleep running operation
R.Wait(time[, prop]);

# ͬʱ���ж����� / Execute several statements in the same time
# statements=Ҫͬʱ���е���伯��(ʹ��{}��Χ) / The statements that will be execute in the same time(surrounded by '{}')
R.Parallel(statements);
```

#### ResMgr - Rabotora��Դ���������� / Rabotora Resource Manager Object
�ṩ�ķ�������:��Դ������صĹ���  
Type of provided methods: Functions of resource loading  
�ṩ�Ĳ��ֳ��÷���:  /  Part of common-used provided methods:  
```php
# ������Դ / Load a resource
# type=��Դ������ / Resource Type
# id=������ɺ�ʹ�õ���Դid / Resource id that will be used after loading
# resPath=��Դ��·��(��ʽ:'���ݰ���/��Դ�ļ���ԭʼ��(������չ��)') / Resource Path (Format: 'Data Package Name/The original name of the resource(including the extension filename)')
# usingTimes=��Դ��ʹ�ô���(ֵΪ'0'��ʹ��һ��;����/ֵΪ'-1'������ʹ��(֪ͨ�����ռ������ռ�����Դ)) / Using times of the resource('0' means use once; '-1' or leave blank means long-time using(notify the garbage collector not to dispose this resource))
ResMgr.LoadRes(type, id, resPath[, usingTimes]);
```
ʹ�õ���Դ����: / Resource type that being used:  
- ResType.Image: ��̬ͼƬ��Դ / Static Image Resources
- ResType.DyImage: ��̬ͼƬ��Դ / Dynamic Image Resources
- ResType.Audio: ��Ƶ��Դ / Audio Resources
- ResType.Video: ��Ƶ��Դ / Video Resources
- ResType.Font: ������Դ / Font Resources
- ResType.Binary: ��������Դ / Binary Resources
- ResType.Other: ����������Դ / Resources of other types

--------
���༭����:2021.4.18  
Last Edited Date:4/18/2021