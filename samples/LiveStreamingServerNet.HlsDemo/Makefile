IMG ?= live-streaming-server-net-hls-demo
PORT ?= 8080
RTMP_PORT ?= 1935

.PHONY: build
build:
	docker build -t $(IMG) -f ./Dockerfile ../../

.PHONY: run
run:
	docker run -p $(PORT):8080 -p $(RTMP_PORT):1935 -d $(IMG)