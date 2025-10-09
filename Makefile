# ==============================================================
# Makefile for CENG795 HW1 - Raytracer (.NET 9.0)
# Supports fast build and single-file publish
# ==============================================================

TARGET = raytracer
BIN_DIR = bin
BUILD_DIR = $(BIN_DIR)\Release\net9.0
PUBLISH_DIR = $(BUILD_DIR)\win-x64\publish
DOTNET = dotnet
FLAGS = -nowarn:*

MAKEFLAGS += --no-print-directory

# --------------------------------------------------------------
# FAST BUILD (no single-file, debuggable, quick iterations)
# --------------------------------------------------------------
fast:
	@$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:AssemblyName=$(TARGET) $(FLAGS)

run-fast:
	@$(PUBLISH_DIR)\$(TARGET).exe ../hw1/inputs/simple.json

fastrun:
	@make fast
	@make run-fast

# --------------------------------------------------------------
# SINGLE-FILE PUBLISH BUILD (submission-ready)
# --------------------------------------------------------------
single:
	@echo "=== Building single-file, self-contained version ==="
	$(DOTNET) publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:AssemblyName=$(TARGET)
	@copy "$(PUBLISH_DIR)\$(TARGET).exe" ".\$(TARGET).exe" >nul
	@echo "=== Single-file build complete. ==="

run-single:
	@echo "=== Running single-file executable ==="
	@.\$(TARGET).exe ../hw1/inputs/simple.json

# --------------------------------------------------------------
# CLEANUP
# --------------------------------------------------------------
clean:
	@echo "Cleaning..."
	@if exist "$(BIN_DIR)" rmdir /s /q "$(BIN_DIR)"
	@if exist "obj" rmdir /s /q "obj"
	@if exist "$(TARGET).exe" del "$(TARGET).exe"
	@echo "Clean complete."
