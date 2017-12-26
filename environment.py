import json
import logging
import subprocess


git_executable = "git"
msbuild_2017_executable = "msbuild_2017"
nuget_executable = "nuget"


def configure_logging(verbose):
	log_level = logging.DEBUG if verbose else logging.INFO
	log_format = "[{levelname}] {message}"
	logging.basicConfig(level = log_level, format = log_format, style = "{")
	logging.addLevelName(logging.DEBUG, "Debug")
	logging.addLevelName(logging.INFO, "Info")
	logging.addLevelName(logging.WARNING, "Warning")
	logging.addLevelName(logging.ERROR, "Error")
	logging.addLevelName(logging.CRITICAL, "Critical")


def get_version():
	with open("Version.json", "r") as version_file:
		version = json.loads(version_file.read())
	version["revision"] = subprocess.check_output([ git_executable, "rev-parse", "--verify", "--short=10", "HEAD" ]).decode("utf-8").strip()
	version["branch"] = subprocess.check_output([ git_executable, "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	version["full"] = version["full_format"].format(**version)
	version["numeric"] = version["numeric_format"].format(**version)
	return version
