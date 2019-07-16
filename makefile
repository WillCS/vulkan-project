COMPILE_GLSL = glslangValidator -V
COMPILE_MONO = msbuild
EXECUTE_MONO = mono
SRC_DIR = src
SHADER_DIR = shaders
CSPROJ_FILE = project.csproj
OUTPUT_DIR = bin/Debug/net472
EXECUTABLE = project.exe

build: compileShaders
	$(COMPILE_MONO) $(CSPROJ_FILE)

restore:
	dotnet restore

run: build
	$(EXECUTE_MONO) $(OUTPUT_DIR)/$(EXECUTABLE)

compileShaders: $(SRC_DIR)/$(SHADER_DIR)/triangle.frag $(SRC_DIR)/$(SHADER_DIR)/triangle.vert
	$(COMPILE_GLSL) $(SRC_DIR)/$(SHADER_DIR)/triangle.frag
	$(COMPILE_GLSL) $(SRC_DIR)/$(SHADER_DIR)/triangle.vert

clean:
	rm -rf bin
	rm mono_crash*
	rm *\.spv