import logging
import os


logger = logging.getLogger("Main")


def configure_argument_parser(environment, configuration, subparsers): # pylint: disable = unused-argument
	return subparsers.add_parser("metadata", help = "generate the project metadata files")


def run(environment, configuration, arguments): # pylint: disable = unused-argument
	output_directory = os.path.join(configuration["artifact_directory"], "Metadata")
	write_product_information(configuration, output_directory, simulate = arguments.simulate)


def write_product_information(configuration, output_directory, simulate):
	information = {
		"assembly_product": configuration["project_name"],
		"assembly_company": configuration["organization"],
		"assembly_copyright": configuration["copyright"],
		"assembly_version": configuration["project_version"]["numeric"],
		"assembly_file_version": configuration["project_version"]["numeric"],
		"assembly_informational_version": configuration["project_version"]["full"],
	}

	if not simulate:
		os.makedirs(output_directory, exist_ok = True)

	logger.info("Writing ProductInformation.cs")
	for key, value in information.items():
		logger.info("%s: '%s'", key, value)

	with open(os.path.join("Metadata", "ProductInformation.template.cs"), mode = "r", encoding = "utf-8") as template_file:
		file_content = template_file.read()
	file_content = file_content.format(**information)
	if not simulate:
		with open(os.path.join(output_directory, "ProductInformation.cs"), mode = "w", encoding = "utf-8") as product_file:
			product_file.write(file_content)
