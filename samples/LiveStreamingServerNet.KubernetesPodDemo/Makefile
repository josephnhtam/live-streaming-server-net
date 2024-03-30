NAMESPACE ?= live-streaming-server-net
IMG ?= live-streaming-server-net-pod-demo

.PHONY: build
build:
	docker build -t $(IMG) -f ./Dockerfile ../../

.PHONY: deploy_docker_desktop
deploy_docker_desktop:
	CD ./Manifests/overlays/docker_desktop && kustomize edit set image image=$(IMG)
	CD ./Manifests/overlays/docker_desktop && kustomize edit set namespace $(NAMESPACE)
	kustomize build --enable-helm ./Manifests/overlays/docker_desktop | kubectl apply -f -
	
.PHONY: undeploy_docker_desktop
undeploy_docker_desktop:
	CD ./Manifests/overlays/docker_desktop && kustomize edit set image image=$(IMG)
	CD ./Manifests/overlays/docker_desktop && kustomize edit set namespace $(NAMESPACE)
	kustomize build --enable-helm ./Manifests/overlays/docker_desktop | kubectl delete -f -