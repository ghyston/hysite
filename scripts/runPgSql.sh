#!/bin/bash
docker run -it -d \
-e POSTGRES_USER=maingalaxyroot \
-e POSTGRES_PASSWORD=themostcomplexone \
-e POSTGRES_DB=hysite_local_db \
-v pgdata:/var/lib/postgresql/data \
--name hysite_pgsql \
-p 5432:5432 \
postgres