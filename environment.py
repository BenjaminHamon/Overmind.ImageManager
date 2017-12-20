import json
import subprocess


git_executable = "git"
msbuild_2017_executable = "msbuild_2017"
nuget_executable = "nuget"


def get_version():
	with open("Version.json", "r") as version_file:
		version = json.loads(version_file.read())
	version["revision"] = subprocess.check_output([ git_executable, "rev-parse", "--verify", "--short=10", "HEAD" ]).decode("utf-8").strip()
	version["branch"] = subprocess.check_output([ git_executable, "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	version["full"] = version["full_format"].format(**version)
	version["numeric"] = version["numeric_format"].format(**version)
	return version
