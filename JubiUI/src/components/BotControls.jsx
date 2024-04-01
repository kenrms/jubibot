import React, { useState, useEffect, useRef } from "react";
import { FaRunning, FaStop, FaSync } from "react-icons/fa";
import styled, { keyframes } from "styled-components";
import configDevelopment from "../../config/config.dev.json";
import configProduction from "../../config/config.prod.json";
import AIConfigurationForm from "./AIConfigurationForm";

const config =
  process.env.NODE_ENV === "production" ? configProduction : configDevelopment;

const StatusIndicator = styled.span`
  color: ${(props) => (props.running ? "green" : "red")};
`;

const spinAnimation = keyframes`
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
`;

const RefreshIcon = styled(FaSync)`
  animation: ${(props) => (props.isLoading ? spinAnimation : "none")} 1s linear
    infinite;
  cursor: pointer;
  margin-left: 5px; /* Add margin between status and refresh button */
`;

const ControlGroup = styled.div`
  margin-bottom: 10px;
`;

const Button = styled.button`
  margin-right: 10px;
`;

function BotControls() {
  const [status, setStatus] = useState(null);
  const [error, setError] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  const fetchStatus = () => {
    setIsLoading(true);

    fetch(`${config.apiBaseUrl}/status`)
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
      })
      .then((data) => {
        setStatus(data.running);
      })
      .catch((error) => {
        console.error("There was an error!", error);
        setError(error.toString());
      })
      .finally(() => setIsLoading(false));
  };

  useEffect(() => {
    fetchStatus();
    const intervalId = setInterval(fetchStatus, 30000);

    return () => clearInterval(intervalId);
  }, []);

  const startBot = () => {
    fetch(`${config.apiBaseUrl}/start/`, {
      method: "POST",
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        refreshStatus();
      })
      .catch((error) => {
        console.error("There was an error!", error);
        setError(error.toString());
      });
  };

  const stopBot = () => {
    fetch(`${config.apiBaseUrl}/stop/`, {
      method: "POST",
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        refreshStatus();
      })
      .catch((error) => {
        console.error("There was an error!", error);
        setError(error.toString());
      });
  };

  const refreshStatus = () => {
    setError(null);
    fetchStatus();
  };

  return (
    <div>
      {error && <p>Error: {error}</p>}

      <div>
        {status !== null ? (
          <p>
            The bot is currently{" "}
            <StatusIndicator running={status}>
              {status ? <FaRunning /> : <FaStop />}
              {status ? "running" : "not running"}
            </StatusIndicator>
            <RefreshIcon isLoading={isLoading} onClick={refreshStatus} />
          </p>
        ) : (
          isLoading && <p>Loading...</p>
        )}
      </div>

      <ControlGroup>
        <Button onClick={startBot} disabled={status}>
          Start Bot
        </Button>
        <Button onClick={stopBot} disabled={!status}>
          Stop Bot
        </Button>
      </ControlGroup>

      <AIConfigurationForm />
    </div>
  );
}

export default BotControls;
