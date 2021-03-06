#!/usr/bin/env bash

EXPOSED_PORT=8001
echo 'Connect your browser to 127.0.0.1:'${EXPOSED_PORT}

# use this line on mac/linux.
# docker run --rm --name mkdocs-material -it -p ${EXPOSED_PORT}:8000 -v ${PWD}:/docs -v squidfunk/mkdocs-material

# use this line on windows to escape the path conversion on windows.  
# https://stackoverflow.com/questions/50608301/docker-mounted-volume-adds-c-to-end-of-windows-path-when-translating-from-linux
docker run --rm --name mkdocs-material -it -p ${EXPOSED_PORT}:8000 -v /${PWD}:/docs squidfunk/mkdocs-material