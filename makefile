COMPILE_GLSL = glslangValidator -V
COMPILE_MONO = msbuild
EXECUTE_MONO = mono
SRC_DIR = src
SHADER_DIR = shaders
CSPROJ_FILE = project.csproj
OUTPUT_DIR = bin
MONO_OUTPUT_DIR = Debug/net472
EXECUTABLE = project.exe

build: compileShaders
	$(COMPILE_MONO) $(CSPROJ_FILE)

restore:
	dotnet restore

run: build
	$(EXECUTE_MONO) $(OUTPUT_DIR)/$(MONO_OUTPUT_DIR)/$(EXECUTABLE)

compileShaders: $(SRC_DIR)/$(SHADER_DIR)/triangle.frag $(SRC_DIR)/$(SHADER_DIR)/triangle.vert $(OUTPUT_DIR)
	$(COMPILE_GLSL) $(SRC_DIR)/$(SHADER_DIR)/triangle.frag -o $(OUTPUT_DIR)/frag.spv
	$(COMPILE_GLSL) $(SRC_DIR)/$(SHADER_DIR)/triangle.vert -o $(OUTPUT_DIR)/vert.spv

$(OUTPUT_DIR):
	mkdir $(OUTPUT_DIR)

clean:
	rm -rf $(OUTPUT_DIR)
	rm mono_crash\.*\.json