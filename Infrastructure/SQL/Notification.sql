CREATE TABLE IF NOT EXISTS public.notification
(
    id uuid NOT NULL,
    "idToDoUser" uuid NOT NULL,
    type character varying(64) COLLATE pg_catalog."default" NOT NULL,
    text character varying(64) COLLATE pg_catalog."default" NOT NULL,
    "scheduledAt" timestamp without time zone NOT NULL,
    "isNotified" boolean NOT NULL,
    "notifiedAt" timestamp without time zone,
    CONSTRAINT pk_notification PRIMARY KEY (id),
    CONSTRAINT "fk_notifikation_toDoUser" FOREIGN KEY ("idToDoUser")
        REFERENCES public."toDoUser" (id) MATCH FULL
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)


CREATE INDEX IF NOT EXISTS "idToDoUser"
    ON public.notification USING btree
    ("idToDoUser" ASC NULLS LAST);
