import logging
import os
import shutil
import sys

sys.path.insert(0, os.path.join(sys.path[0], ".."))

import development.configuration # pylint: disable = wrong-import-position
import development.environment # pylint: disable = wrong-import-position


logger = logging.getLogger("Main")


def main():
	with development.environment.execute_in_workspace(__file__):
		environment_instance = development.environment.load_environment()
		configuration_instance = development.configuration.load_configuration(environment_instance) # pylint: disable = unused-variable
		development.environment.configure_logging(environment_instance, None)

		global_status = { "success": True }

		check_commands(global_status)
		check_software(global_status, environment_instance)

	if not global_status["success"]:
		raise RuntimeError("Check found issues")


def check_commands(global_status):
	command_list = development.configuration.load_commands()

	for command in command_list:
		if "exception" in command:
			global_status["success"] = False
			logger.error("Command '%s' is unavailable", command["module_name"], exc_info = command["exception"])
			print("")


def check_software(global_status, environment_instance):
	msbuild_executable = environment_instance.get("msbuild_executable", None)
	if msbuild_executable is None or not shutil.which(msbuild_executable):
		global_status["success"] = False
		logger.error("MSBuild is required (Path: '%s')", msbuild_executable)

	nuget_executable = environment_instance.get("nuget_executable", None)
	if nuget_executable is None or not shutil.which(nuget_executable):
		global_status["success"] = False
		logger.error("NuGet is required (Path: '%s')", nuget_executable)

	vstest_executable = environment_instance.get("vstest_executable", None)
	if vstest_executable is None or not shutil.which(vstest_executable):
		global_status["success"] = False
		logger.error("Visual Studio Test Console is required (Path: '%s')", vstest_executable)


if __name__ == "__main__":
	main()
