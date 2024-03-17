NAMESPACE ?= live-streaming-server-net
IMG ?= live-streaming-server-net-operator

.PHONY: install-kubops-cli
install-kubops-cli:
	dotnet tool install KubeOps.Cli

.PHONY: manifest
manifest:
	dotnet kubeops generate operator live-streaming-server-net --out ./Manifests
	
.PHONY: build
build:
	docker build -t $(IMG) -f ./Dockerfile ../../
	
.PHONY: deploy
deploy:
	CD ./Manifests && kustomize edit set image operator=$(IMG)
	CD ./Manifests && kustomize edit set namespace $(NAMESPACE)
	kustomize build ./Manifests | kubectl apply -f -
	
.PHONY: undeploy
undeploy:
	CD ./Manifests && kustomize edit set image operator=$(IMG)
	CD ./Manifests && kustomize edit set namespace $(NAMESPACE)
	kustomize build ./Manifests | kubectl delete -f -