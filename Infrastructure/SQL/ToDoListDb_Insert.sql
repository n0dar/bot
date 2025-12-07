CREATE EXTENSION IF NOT EXISTS pgcrypto;

INSERT INTO public."toDoUser" (id, "telegramUserId", "telegramUserName", "registeredAt") VALUES
(gen_random_uuid(), 123456789, 'user1_test', CURRENT_TIMESTAMP - INTERVAL '2 days'),
(gen_random_uuid(), 987654321, 'user2_telegram', CURRENT_TIMESTAMP - INTERVAL '1 day'),
(gen_random_uuid(), 555666777, 'alice_dev', CURRENT_TIMESTAMP);

INSERT INTO public."toDoList" (id, name, "idToDoUser", "createdAt") VALUES
(gen_random_uuid(), 'Рабочие задачи', (SELECT id FROM public."toDoUser" WHERE "telegramUserId" = 123456789), CURRENT_TIMESTAMP - INTERVAL '1 day'),
(gen_random_uuid(), 'Личные дела', (SELECT id FROM public."toDoUser" WHERE "telegramUserId" = 987654321), CURRENT_TIMESTAMP),
(gen_random_uuid(), 'Домашние дела', (SELECT id FROM public."toDoUser" WHERE "telegramUserId" = 555666777), CURRENT_TIMESTAMP - INTERVAL '12 hours');

INSERT INTO public."toDoItem" (id, "idToDoUser", name, "createdAt", state, "stateChangedAt", deadline, "idToDoList") VALUES
(gen_random_uuid(), (SELECT id FROM public."toDoUser" WHERE "telegramUserId" = 123456789), 'Написать отчет', CURRENT_TIMESTAMP - INTERVAL '20 hours', true, CURRENT_TIMESTAMP - INTERVAL '2 hours', '2025-12-05', (SELECT id FROM public."toDoList" WHERE name = 'Рабочие задачи')),
(gen_random_uuid(), (SELECT id FROM public."toDoUser" WHERE "telegramUserId" = 123456789), 'Позвонить клиенту', CURRENT_TIMESTAMP - INTERVAL '15 hours', false, NULL, '2025-12-06', (SELECT id FROM public."toDoList" WHERE name = 'Рабочие задачи')),
(gen_random_uuid(), (SELECT id FROM public."toDoUser" WHERE "telegramUserId" = 123456789), 'Протестировать API', CURRENT_TIMESTAMP - INTERVAL '10 hours', false, NULL, '2025-12-07', NULL);
