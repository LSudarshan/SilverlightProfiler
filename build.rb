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

task :package do
  mkdir_p "deploy"
  copy_files "*.bat", "deploy"
  copy_files "SilverlightProfiler/bin/Debug/*.exe", "deploy"
  copy_files "SilverlightProfiler/bin/debug/*.dll", "deploy"
  copy_files "RemoveStrongNames/bin/Debug/*.exe", "deploy"
  copy_files "RemoveStrongNames/bin/debug/*.dll", "deploy"
end

task :all => [:test, :ft, :package]

