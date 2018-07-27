# SimpleSpider 一个简单的爬虫
SimpleSpider 由 爬虫主程序，爬虫脚本，入库管理 3个部分组成

SimpleSpider.exe 负责解析运行脚本文件，使用方法
	SimpleSpider.exe config.ss

config.ss 简单的爬虫脚本，由命令序列构成，命令由 SimpleSpider.Command 提供支持
```
engine
	command [arg1] [arg2] ....
			sub_command
```
