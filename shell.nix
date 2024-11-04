with import <nixpkgs> {};
let
  port = 8000;
in
pkgs.mkShell {
  nativeBuildInputs = [
      tilt
      dapr-cli
  ];

  DOCKER_BUILDKIT = 0;

  LOG_LEVEL = 4;
  CLIENT_PORT = port + 80;
  SERVER_PORT = port + 85;
  TILT_PORT = port + 50;

  shellHook = ''
    export TILT_ENV=$USER
    export TILT_NAMESPACE=spmsa
  '';
}