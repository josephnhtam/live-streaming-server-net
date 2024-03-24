NAMESPACE ?= live-streaming-server-net
IMG ?= live-streaming-server-net-operator

.PHONY: install-kubops-cli
install-kubops-cli:
	dotnet tool install KubeOps.Cli

.PHONY: manifest
manifest:
	dotnet kubeops generate operator live-streaming-server-net --out ./Manifests/base
ifeq ($(OS),Windows_NT) 
	del .\Manifests\base\Dockerfile
else
	rm -f ./Manifests/base/Dockerfile
endif
	
.PHONY: build
build:
	docker build -t $(IMG) -f ./Dockerfile ../../
	
.PHONY: deploy_docker_desktop
deploy_docker_desktop:
	CD ./Manifests/overlays/docker_desktop && kustomize edit set image operator=$(IMG)
	CD ./Manifests/overlays/docker_desktop && kustomize edit set namespace $(NAMESPACE)
	kustomize build ./Manifests/overlays/docker_desktop | kubectl apply -f -
	
.PHONY: undeploy_docker_desktop
undeploy_docker_desktop:
	CD ./Manifests/overlays/docker_desktop && kustomize edit set image operator=$(IMG)
	CD ./Manifests/overlays/docker_desktop && kustomize edit set namespace $(NAMESPACE)
	kustomize build ./Manifests/overlays/docker_desktop | kubectl delete -f -