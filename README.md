# SimpleSpider
Use config
```
engine
	task souhu
	rang 43 44
		rang-select 1 2 http://v2.sohu.com/public-api/feed?scene=CHANNEL&sceneId={$0}&page={$1}&size=20
		foreach
			data url=*
			httper utf8
			json .
			foreach
				json id=id title=title url=$http://www.sohu.com/a/{id}_{authorId}
				data url
				httper utf8
				xpath title=//*[@id="article-container"]/div[2]/div[1]/div[1]/h1 content=//*[@id="mp-editor"]=html
				htmlclear content
				export-json souhu.json title,content
	 
```
