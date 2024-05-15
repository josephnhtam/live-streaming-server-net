IMG ?= live-streaming-server-net-basic-demo
PORT ?= 1935

.PHONY: build
build:
	docker build -t $(IMG) -f ./Dockerfile ../../

.PHONY: run
run:
	docker run -p $(PORT):1935 -d $(IMG)