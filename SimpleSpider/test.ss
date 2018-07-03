engine
	task 搜狐新闻
	data source=搜狐新闻
	data tag=时政
	data sceneId=1461
	range 1 2
	select http://v2.sohu.com/public-api/feed?scene=CHANNEL&sceneId={sceneId}&page={$0}&size=20
	foreach
		httper utf-8