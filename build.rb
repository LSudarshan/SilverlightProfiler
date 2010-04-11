require 'build_tools'

task :compile do
  msbuild "SilverlightProfiler"
end

task :test => [:compile, :vdir] do
  nunit "SilverlightProfilerUnitTest"
end

task :ft do
  sh "start \"c:\\program files\\internet explorer\\iexplore.exe\" http://localhost/SilverlightTestApplication/SilverlightTestApplicationTestPage.aspx"
end

task :vdir do
  vdir "e:\\projects\\silverlightprofiler\\SilverlightTestApplication.Web", "SilverlightTestApplication"
end

task :all => [:test, :ft]

