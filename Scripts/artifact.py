import argparse
import glob
import json
import logging
import os
import shutil
import zipfile


def configure_argument_parser(environment, configuration, subparsers):

	def parse_key_value_parameter(argument_value):
		key_value = argument_value.split("=")
		if len(key_value) != 2:
			raise argparse.ArgumentTypeError("invalid key value parameter: '%s'" % argument_value)
		return (key_value[0], key_value[1])

	command_list = [ "show", "package", "verify", "upload" ]

	parser = subparsers.add_parser("artifact", formatter_class = argparse.RawTextHelpFormatter,
		help = "execute commands related to build artifacts")
	parser.add_argument("artifact", choices = configuration["artifacts"].keys(),
		metavar = "<artifact>", help = "set an artifact definition to use for the commands")
	parser.add_argument("--command", choices = command_list, nargs = "+", dest = "artifact_commands",
		metavar = "<command>", help = "set the command(s) to execute for the artifact" + "\n" + "(%s)" % ", ".join(command_list))
	parser.add_argument("--parameters", nargs = "*", type = parse_key_value_parameter, default = [],
		metavar = "<key=value>", help = "set parameters for the artifact")
	return parser


def run(environment, configuration, arguments):
	parameters = {
		"project": configuration["project"],
		"version": configuration["project_version"]["full"],
	}

	parameters.update(arguments.parameters)

	artifact = configuration["artifacts"][arguments.artifact]
	artifact_name = artifact["file_name"].format(**parameters)
	artifact_fileset = configuration["filesets"][artifact["fileset"]]

	local_artifact_path = os.path.join(".artifacts", artifact["path_in_repository"], artifact_name)
	remote_artifact_path = os.path.join(environment["artifact_repository"], configuration["project"], artifact["path_in_repository"], artifact_name)

	if "show" in arguments.artifact_commands:
		show(artifact_name, artifact_fileset, parameters)
		logging.info("")
	if "package" in arguments.artifact_commands:
		package(local_artifact_path, artifact_fileset, parameters, arguments.simulate)
		logging.info("")
	if "verify" in arguments.artifact_commands:
		verify(local_artifact_path)
		logging.info("")
	if "upload" in arguments.artifact_commands:
		upload(local_artifact_path, remote_artifact_path, arguments.simulate, arguments.results)
		logging.info("")


def show(artifact_name, fileset, parameters):
	logging.info("Artifact %s", artifact_name)

	_, all_files = list_files(fileset, parameters)
	for file_path in all_files:
		logging.info("%s", file_path)


def package(artifact_path, fileset, parameters, simulate):
	logging.info("Packaging artifact %s", artifact_path)

	artifact_directory = os.path.dirname(artifact_path)
	if not simulate and not os.path.isdir(artifact_directory):
		os.makedirs(artifact_directory)

	path_in_workspace, all_files = list_files(fileset, parameters)
	if len(all_files) == 0:
		raise RuntimeError("The artifact is empty")

	all_files = [ (source, source[ len(path_in_workspace) + 1 : ]) for source in all_files ]

	if simulate:
		for source, destination in all_files:
			logging.info("Adding %s", source)
	else:
		with zipfile.ZipFile(artifact_path + ".zip.tmp", "w", zipfile.ZIP_DEFLATED) as artifact_file:
			for source, destination in all_files:
				logging.info("Adding %s", source)
				artifact_file.write(source, destination)
		shutil.move(artifact_path + ".zip.tmp", artifact_path + ".zip")


def verify(artifact_path):
	logging.info("Verifying artifact %s", artifact_path)

	with zipfile.ZipFile(artifact_path + ".zip", 'r') as artifact_file:
		if artifact_file.testzip():
			raise RuntimeError('Artifact package is corrupted')


def upload(local_artifact_path, remote_artifact_path, simulate, result_file_path):
	logging.info("Uploading artifact '%s' to '%s'", local_artifact_path, remote_artifact_path)

	remote_artifact_directory = os.path.dirname(remote_artifact_path)
	if not simulate and not os.path.isdir(remote_artifact_directory):
		os.makedirs(remote_artifact_directory)

	if not simulate:
		shutil.copyfile(local_artifact_path + ".zip", remote_artifact_path + ".zip.tmp")
		shutil.move(remote_artifact_path + ".zip.tmp", remote_artifact_path + ".zip")

	if result_file_path:
		results = load_results(result_file_path)
		results["artifacts"].append({ "name": os.path.basename(remote_artifact_path), "path": remote_artifact_path })
		if not simulate:
			save_results(result_file_path, results)


def list_files(fileset, parameters):
	all_files = []
	path_in_workspace = fileset["path_in_workspace"].format(**parameters)
	for file_pattern in fileset["file_patterns"]:
		all_files += glob.glob(os.path.join(path_in_workspace, file_pattern.format(**parameters)))
	return path_in_workspace, sorted(file_path.replace("\\", "/") for file_path in all_files)


def load_results(result_file_path):
	if not os.path.isfile(result_file_path):
		return { "artifacts": [] }
	with open(result_file_path, "r") as result_file:
		return json.load(result_file)


def save_results(result_file_path, result_data):
	if not os.path.isdir(os.path.dirname(result_file_path)):
		os.makedirs(os.path.dirname(result_file_path))
	with open(result_file_path, "w") as result_file:
		json.dump(result_data, result_file, indent = 4)
