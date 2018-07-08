import subprocess

import artifact
import clean
import compile
import metadata


def get_command_list():
	return [ artifact, clean, compile, metadata ]


def load_configuration(environment):
	configuration = {
		"project": "Overmind.ImageManager",
		"project_name": "Overmind Image Manager",
		"project_version": { "identifier": "1.0" },

		"application_list": [ "WindowsClient", "WallpaperService" ],
		"configuration_list": [ "Debug", "Release" ],
	}

	configuration["project_version"]["revision"] = subprocess.check_output([ environment["git_executable"], "rev-parse", "--short=10", "HEAD" ]).decode("utf-8").strip()
	configuration["project_version"]["branch"] = subprocess.check_output([ environment["git_executable"], "rev-parse", "--abbrev-ref", "HEAD" ]).decode("utf-8").strip()
	configuration["project_version"]["numeric"] = "{identifier}".format(**configuration["project_version"])
	configuration["project_version"]["full"] = "{identifier}-{revision}".format(**configuration["project_version"])

	configuration["filesets"] = {
		"binaries": {
			"path_in_workspace": ".build/{application}/bin/{configuration}",
			"file_patterns": [ "{project}.{application}.exe", "{project}.{application}.exe.config", "*.dll", "*.pdb", "*.xml", ],
		},
		"package": {
			"path_in_workspace": ".build/{application}/bin/{configuration}",
			"file_patterns": [ "{project}.{application}.exe", "{project}.{application}.exe.config", "*.dll", "*.pdb", "*.xml", "Credits.txt" ],
		},
	}

	configuration["artifacts"] = {
		"binaries": {
			"file_name": "{project}.{application}_{version}_Binaries_{configuration}",
			"path_in_repository": "binaries",
			"fileset": "binaries",
		},
		"package": {
			"file_name": "{project}.{application}_{version}_Package_{configuration}",
			"path_in_repository": "packages",
			"fileset": "package",
		},
	}

	return configuration
