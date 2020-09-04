engine
	var url=http://caipiao.163.com/award/cqssc/20180613.html
	# 将url变量输出到pipelineInput
	var url
	# 从pipelineInput中获取请求url
	httper utf-8
	xpath //*[@id="mainArea"]/div[1]/table/tr
	foreach
		xpath start=//td[1]=text winNum=//td[2]=text
		if start winNum
			replace winNum \s
			result start winNum
	export-json test.json