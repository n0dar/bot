BEGIN;

CREATE TABLE IF NOT EXISTS public."toDoItem"
(
    id uuid NOT NULL,
    "idToDoUser" uuid NOT NULL,
    name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "createdAt" timestamp without time zone NOT NULL,
    state boolean NOT NULL,
    "stateChangedAt" timestamp without time zone,
    deadline date NOT NULL,
    "idToDoList" uuid,
    CONSTRAINT "pk_toDoItem" PRIMARY KEY (id, "idToDoUser")
);

CREATE TABLE IF NOT EXISTS public."toDoList"
(
    id uuid NOT NULL,
    name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "idToDoUser" uuid NOT NULL,
    "createdAt" timestamp without time zone NOT NULL,
    CONSTRAINT "pk_toDoList" PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS public."toDoUser"
(
    id uuid NOT NULL,
    "telegramUserId" bigint NOT NULL,
    "telegramUserName" character varying(32) COLLATE pg_catalog."default" NOT NULL,
    "registeredAt" timestamp without time zone,
    CONSTRAINT "pk_toDoUser" PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS "i_telegramUserId"
    ON public."toDoUser" USING btree
    ("telegramUserId" ASC NULLS LAST);

ALTER TABLE IF EXISTS public."toDoItem"
    ADD CONSTRAINT "fk_toDoItem_toDoList" FOREIGN KEY ("idToDoList")
    REFERENCES public."toDoList" (id) MATCH FULL
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

CREATE INDEX IF NOT EXISTS "i_toDoItem.idToDoList"
    ON public."toDoItem" USING btree
    ("idToDoList" ASC NULLS LAST);

ALTER TABLE IF EXISTS public."toDoItem"
    ADD CONSTRAINT "fk_toDoItem_toDoUser" FOREIGN KEY ("idToDoUser")
    REFERENCES public."toDoUser" (id) MATCH FULL
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

CREATE INDEX IF NOT EXISTS "i_toDoItem.idToDoUser"
    ON public."toDoItem" USING btree
    ("idToDoUser" ASC NULLS LAST);


ALTER TABLE IF EXISTS public."toDoList"
    ADD CONSTRAINT "fk_toDoList_toDoUser" FOREIGN KEY ("idToDoUser")
    REFERENCES public."toDoUser" (id) MATCH FULL
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;
	
CREATE INDEX IF NOT EXISTS "i_toDoList.idToDoUser"
    ON public."toDoList" USING btree
    ("idToDoUser" ASC NULLS LAST);
END;