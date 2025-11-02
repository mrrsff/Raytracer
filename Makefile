TARGET = raytracer
BIN_DIR = bin
BUILD_DIR = $(BIN_DIR)/Release/net9.0
PUBLISH_DIR = $(BUILD_DIR)\win-x64\publish
DOTNET = dotnet
FLAGS = --property WarningLevel=0 /p:Optimize=true

MAKEFLAGS += --no-print-directory

INPUTS_FOLDER = hw2\inputs
OUTPUTS_FOLDER = hw2\outputs
ARGS = spheres.json

all: linux

fast:
	@$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:AssemblyName=$(TARGET) $(FLAGS)

fastrun: fast
	$(PUBLISH_DIR)\$(TARGET).exe .\$(INPUTS_FOLDER)\$(ARGS)

single:
	$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=$(TARGET)
	@copy "$(PUBLISH_DIR)\$(TARGET).exe" ".\$(TARGET).exe" >nul

compare: fastrun
	@py imageCompare.py .\$(OUTPUTS_FOLDER)\$(ARGS:.json=.png) .\Outputs\$(ARGS:.json=.png)

run:
	@$(PUBLISH_DIR)\$(TARGET).exe $(ARGS)

linux:
	@$(DOTNET) publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=$(TARGET) $(FLAGS)
	@make copy-linux

copy-linux: 
	@cp "$(BUILD_DIR)/linux-x64/publish/$(TARGET)" "./$(TARGET)"

clean:
	@echo "Cleaning..."
	@if exist "$(BIN_DIR)" rmdir /s /q "$(BIN_DIR)"
	@if exist "obj" rmdir /s /q "obj"
	@if exist "$(TARGET).exe" del "$(TARGET).exe"
	@echo "Clean complete."

tar:
	@mkdir Tarfile || true
	@rm -f Tarfile/raytracer.tar.gz
	@tar -czf ./Tarfile/raytracer.tar.gz \
		--exclude=hw1 \
		--exclude=Outputs \
		--exclude=.git \
		--exclude=.idea \
		--exclude=bin \
		--exclude=obj \
		--exclude=.gitignore \
		--exclude=Tarfile \
		--exclude=raytracer \
		.


.PHONY: all fast fastrun single run-single clean linux copy-linux compare tar