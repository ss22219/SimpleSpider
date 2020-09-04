engine
	# pipelineInput为集合对象，遍历集合并使用当前项替换参数中的占位符
	select http://v2.sohu.com/public-api/feed?scene=CATEGORY&sceneId={sceneId}&page={$0}&size=20
	foreach
		# 设置变量url为pipelineInput值
		var url=${pipeline}
		break
	var t=1