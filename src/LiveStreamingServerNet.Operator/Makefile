.PHONY: install-kubops-cli
install-kubops-cli:
	dotnet tool install KubeOps.Cli

.PHONY: manifest
manifest:
	dotnet kubeops generate operator LiveStreamingServerNET.Operator --out ./Manifests