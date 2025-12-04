-- Table: public.toDoUser

-- DROP TABLE IF EXISTS public."toDoUser";

CREATE TABLE IF NOT EXISTS public."toDoUser"
(
    id uuid NOT NULL,
    "telegramUserId" bigint NOT NULL,
    "telegramUserName" character varying(32) COLLATE pg_catalog."default" NOT NULL,
    "registeredAt" timestamp without time zone,
    CONSTRAINT "toDoUser_pkey" PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."toDoUser"
    OWNER to postgres;
-- Index: telegramUserId

-- DROP INDEX IF EXISTS public."telegramUserId";

CREATE UNIQUE INDEX IF NOT EXISTS "telegramUserId"
    ON public."toDoUser" USING btree
    ("telegramUserId" ASC NULLS LAST)
    WITH (fillfactor=100, deduplicate_items=True)
    TABLESPACE pg_default;

-- Table: public.toDoList

-- DROP TABLE IF EXISTS public."toDoList";

CREATE TABLE IF NOT EXISTS public."toDoList"
(
    id uuid NOT NULL,
    name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "idToDoUser" uuid NOT NULL,
    "createdAt" timestamp without time zone NOT NULL,
    CONSTRAINT "toDoList_pkey" PRIMARY KEY (id),
    CONSTRAINT "fk_toDoList_toDoUser" FOREIGN KEY ("idToDoUser")
        REFERENCES public."toDoUser" (id) MATCH FULL
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."toDoList"
    OWNER to postgres;
-- Index: toDoList.idToDoUser

-- DROP INDEX IF EXISTS public."toDoList.idToDoUser";

CREATE INDEX IF NOT EXISTS "toDoList.idToDoUser"
    ON public."toDoList" USING btree
    ("idToDoUser" ASC NULLS LAST)
    WITH (fillfactor=100, deduplicate_items=True)
    TABLESPACE pg_default;


-- Table: public.toDoItem

-- DROP TABLE IF EXISTS public."toDoItem";

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
    CONSTRAINT "toDoItem_pkey" PRIMARY KEY (id, "idToDoUser"),
    CONSTRAINT "fk_toDoItem_toDoList" FOREIGN KEY ("idToDoList")
        REFERENCES public."toDoList" (id) MATCH FULL
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "fk_toDoItem_toDoUser" FOREIGN KEY ("idToDoUser")
        REFERENCES public."toDoUser" (id) MATCH FULL
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."toDoItem"
    OWNER to postgres;
-- Index: fki_fk_toDoItem_toDoList

-- DROP INDEX IF EXISTS public."fki_fk_toDoItem_toDoList";

CREATE INDEX IF NOT EXISTS "fki_fk_toDoItem_toDoList"
    ON public."toDoItem" USING btree
    ("idToDoList" ASC NULLS LAST)
    WITH (fillfactor=100, deduplicate_items=True)
    TABLESPACE pg_default;
-- Index: toDoItem.idToDoList

-- DROP INDEX IF EXISTS public."toDoItem.idToDoList";

CREATE INDEX IF NOT EXISTS "toDoItem.idToDoList"
    ON public."toDoItem" USING btree
    ("idToDoList" ASC NULLS LAST)
    WITH (fillfactor=100, deduplicate_items=True)
    TABLESPACE pg_default;
-- Index: toDoItem.idToDoUser

-- DROP INDEX IF EXISTS public."toDoItem.idToDoUser";

CREATE INDEX IF NOT EXISTS "toDoItem.idToDoUser"
    ON public."toDoItem" USING btree
    ("idToDoUser" ASC NULLS LAST)
    WITH (fillfactor=100, deduplicate_items=True)
    TABLESPACE pg_default;

