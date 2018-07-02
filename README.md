# SimpleSpider 一个简单的爬虫
SimpleSpider 由 爬虫主程序，爬虫脚本，入库管理 3个部分组成

SimpleSpider.exe 负责解析运行脚本文件，使用方法
	SimpleSpider.exe config.ss

config.ss 简单的爬虫脚本，由命令序列构成，命令由 SimpleSpider.Command 提供支持
语法格式：
	command [arg1] [arg2] ....
		sub_command

SimpleSpider.UI 一个粗糙的数据发布管理软件，目前支持DedeCMS文章发布