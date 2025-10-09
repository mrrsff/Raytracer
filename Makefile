
TARGET = raytracer
BIN_DIR = bin
BUILD_DIR = $(BIN_DIR)\Release\net9.0
PUBLISH_DIR = $(BUILD_DIR)\win-x64\publish

DOTNET = dotnet

all: $(TARGET)

$(TARGET):
	@echo Building Raytracer (.NET 9.0)...
	$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=$(TARGET)
	@if exist "$(PUBLISH_DIR)\$(TARGET).exe" (copy "$(PUBLISH_DIR)\$(TARGET).exe" ".\$(TARGET).exe" >nul)
	@echo Build Complete.


run:
	@echo Running Raytracer...
	@$(TARGET).exe simple.json

clean:
	@echo Cleaning...
	@if exist "$(BIN_DIR)" rmdir /s /q "$(BIN_DIR)"
	@if exist "obj" rmdir /s /q "obj"
	@if exist "$(TARGET).exe" del "$(TARGET).exe"
	@echo Clean Complete.
