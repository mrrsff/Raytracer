TARGET = raytracer
BIN_DIR = bin
BUILD_DIR = $(BIN_DIR)/Release/net9.0
PUBLISH_DIR = $(BUILD_DIR)\win-x64\publish
DOTNET = dotnet
FLAGS = --property WarningLevel=0 /p:Optimize=true

INPUTS_FOLDER = hw1\inputs
OUTPUTS_FOLDER = hw1\outputs
ARGS = spheres.json

all: fast

fast:
	@$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:AssemblyName=$(TARGET) $(FLAGS)

fastrun: fast
	@$(PUBLISH_DIR)\$(TARGET).exe .\$(INPUTS_FOLDER)\$(ARGS)

single:
	@echo "=== Building single-file, self-contained version ==="
	$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=$(TARGET)
	@copy "$(PUBLISH_DIR)\$(TARGET).exe" ".\$(TARGET).exe" >nul
	@echo "=== Single-file build complete. ==="

compare: fastrun
	@py imageCompare.py .\$(OUTPUTS_FOLDER)\$(ARGS:.json=.png) .\Outputs\$(ARGS:.json=.png)

run-single:
	@echo "=== Running single-file executable ==="
	@.\$(TARGET).exe $(ARGS) 

linux:
	@echo "=== Building for Linux ==="
	@$(DOTNET) publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=$(TARGET) $(FLAGS)
	@echo "=== Linux build complete. ==="
	@make copy-linux

copy-linux: 
	@echo "Copying Linux executable to current directory..."
	@cp "$(BUILD_DIR)/linux-x64/publish/$(TARGET)" "./$(TARGET)"
	@echo "Copy complete."

clean:
	@echo "Cleaning..."
	@if exist "$(BIN_DIR)" rmdir /s /q "$(BIN_DIR)"
	@if exist "obj" rmdir /s /q "obj"
	@if exist "$(TARGET).exe" del "$(TARGET).exe"
	@echo "Clean complete."

.PHONY: all fast fastrun single run-single clean