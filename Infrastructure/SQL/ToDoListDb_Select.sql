SELECT * FROM public."toDoList" WHERE id = 'd63e4914-b42b-4ee6-8e7d-a327191a3d67'::uuid;

SELECT * FROM public."toDoList" WHERE "idToDoUser" = 'dae3e5c2-b288-4bd4-b943-1bd251c01101'::uuid;

SELECT EXISTS(
SELECT * FROM public."toDoList" l INNER JOIN public."toDoItem" i ON l.id=i."idToDoList" 
WHERE 
	i."idToDoUser" = 'dae3e5c2-b288-4bd4-b943-1bd251c01101'::uuid AND
	l.name = 'Рабочие задачи')
	
