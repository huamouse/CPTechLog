# CPTechLog
参照AspNetCore源码写的自定义日志组件（日志文件）

## 组件引用
Install-Package CPTech.Extensions.Logging.File -Version 1.0.0

## 参数配置
在appsettings.json文件Logging节点下增加：
~~~
    "File": {
      "LogPath": "log", //默认文件地址
      "LogNameFormat": "yyyyMMdd", //默认文件名字
      "LogLevel": {
        "Default": "None",
        "***": "Information" // 需要写入至文件的日志节点名称
      }
    }
~~~

## 修改Program.cs注入日志配置
~~~
                    webBuilder.ConfigureLogging(logging =>
                    {
                        //logging.ClearProviders();
                        //logging.AddConsole();
                        logging.AddFile();
                    });
~~~
