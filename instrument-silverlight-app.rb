require 'rexml/document'
include REXML

path = 'e:\projects\bcg\imd\src\imdclient\imdclient.web\clientbin'
#system("rename #{path}\\imdclient.xap imdclient.zip")

require 'FileUtils'
FileUtils.rm_rf 'temp'
Dir.mkdir('temp')

system("unzip #{path}\\imdclient.xap -d temp")


fail "#{$?}" unless system('silverlightprofiler\bin\debug\silverlightprofiler IMDClient IMDClient.App ApplicationStartup')

system('copy /Y SilverlightProfiler\bin\Debug\SilverlightProfilerRuntime.dll temp')
#system('copy /Y afterModification\IMDClient.dll temp')
#system('copy /Y afterModification\IMD.ContactManagementModule.dll temp')
system('copy /Y afterModification\*.* temp')

doc = Document.new File.new('temp\appmanifest.xaml')
XPath.first(doc, '//Deployment.Parts').add_element('AssemblyPart', {'x:Name'=>'SilverlightProfilerRuntime', 'Source'=>'SilverlightProfilerRuntime.dll'})

File.open('temp\AppManifest.xaml', 'w') do |f|
  doc.write f, 1
end

Dir.chdir 'temp'

system("del #{path}\\imdclient.xap")
system("zip #{path}\\imdclient.xap *.*")

