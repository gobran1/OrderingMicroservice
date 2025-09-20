  #!/bin/bash

  if ! curl -f http://localhost:${GATEWAY_PORT}/health/live; then
      echo "Liveness check failed"
      exit 1
  fi

  echo "Health checks passed"
  exit 0
