COMPILE_GLSL 	= glslangValidator -V
COMPILE_MONO 	= msbuild
EXECUTE_MONO 	= mono
PROJECT_DIR  	= project
TEST_DIR		= test
SRC_DIR 	 	= src
SHADER_DIR   	= shaders
SLN_FILE 		= project.sln
OUTPUT_DIR 		= bin
MONO_OUTPUT_DIR = Debug/net472
EXECUTABLE 		= project.exe
TESTS			= test.dll
MONO			= mono
MONO_DEBUG_ARGS = --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555
NUNIT_CONSOLE	= /home/will/.nuget/packages/nunit.consolerunner/3.10.0/tools/nunit3-console.exe

build: compileShaders
	$(COMPILE_MONO) $(CSPROJ_FILE)

debugBuild: compileShaders
	$(COMPILE_MONO) /p:Configuration=Debug $(CSPROJ_FILE)

test: debugBuild
	$(MONO) $(NUNIT_CONSOLE) $(TEST_DIR)/$(OUTPUT_DIR)/$(MONO_OUTPUT_DIR)/$(TESTS)

restore:
	dotnet restore

run: build
	$(EXECUTE_MONO) $(PROJECT_DIR)/$(OUTPUT_DIR)/$(MONO_OUTPUT_DIR)/$(EXECUTABLE)

debug: debugBuild
	$(EXECUTE_MONO) $(MONO_DEBUG_ARGS) $(PROJECT_DIR)/$(OUTPUT_DIR)/$(MONO_OUTPUT_DIR)/$(EXECUTABLE)

compileShaders: $(PROJECT_DIR)/$(SRC_DIR)/$(SHADER_DIR)/triangle.frag $(PROJECT_DIR)/$(SRC_DIR)/$(SHADER_DIR)/triangle.vert $(PROJECT_DIR)/$(OUTPUT_DIR)
	$(COMPILE_GLSL) $(PROJECT_DIR)/$(SRC_DIR)/$(SHADER_DIR)/triangle.frag -o $(PROJECT_DIR)/$(OUTPUT_DIR)/frag.spv
	$(COMPILE_GLSL) $(PROJECT_DIR)/$(SRC_DIR)/$(SHADER_DIR)/triangle.vert -o $(PROJECT_DIR)/$(OUTPUT_DIR)/vert.spv

$(PROJECT_DIR)/$(OUTPUT_DIR):
	mkdir $(PROJECT_DIR)/$(OUTPUT_DIR)

clean:
	rm -rf $(PROJECT_DIR)/$(OUTPUT_DIR)
	rm -rf $(TEST_DIR)/$(OUTPUT_DIR)
	rm mono_crash\.*\.json