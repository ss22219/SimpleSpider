engine
	task 搜狐新闻
	data source=搜狐新闻
	data tag=时政
	data sceneId=1460
	range 1 3
	select http://v2.sohu.com/public-api/feed?scene=CATEGORY&sceneId={sceneId}&page={$0}&size=20
	foreach
		data url=*
		httper utf-8
		json .
		foreach
			json id=id title=title url=$http://www.sohu.com/a/{id}_{authorId}
			data url
			httper utf-8
			xpath title=/html/head/meta[8]=attr[content] content=//*[@id="mp-editor"]=html
			htmlclear content img
			htmldecode title
			replace content <p[^<>]+?data-role[^<>]+?>.+?</p>
			result source title content tag
	export-sqlserver server=.;Integrated\ Security=True;database=spider article