import React, { useState, useEffect, useRef } from 'react';
import styled from 'styled-components';
import CollapsibleContainer from './CollapsibleContainer';
import configDevelopment from '../../config/config.dev.json';
import configProduction from '../../config/config.prod.json';


const config = process.env.NODE_ENV === 'production' ? configProduction : configDevelopment;

const Label = styled.label`
  display: block;
  margin-bottom: 5px;
`;

const Input = styled.input`
  width: 94%;
  padding: 8px;
  margin-bottom: 10px;
`;

const TextArea = styled.textarea`
  width: 94%;
  height: 300px;
  padding: 8px;
  margin-bottom: 10px;
`;

const Select = styled.select`
  width: 98%;
  padding: 8px;
  margin-bottom: 10px;
`;

const modelOptions = [
    { value: 'gpt-3.5-turbo', label: 'GPT-3.5 Turbo ($)' },
    { value: 'gpt-4', label: 'GPT-4 ($$$$)' }
];


const AIConfigurationForm = () => {
    const [error, setError] = useState(null);
    const [temperature, setTemperature] = useState(0);
    const [maxTokens, setMaxTokens] = useState('');
    const [systemPrompt, setSystemPrompt] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [selectedModel, setSelectedModel] = useState(modelOptions[0].value);

    const debounceTimerRef = useRef(null);

    const SliderContainer = styled.div`
        margin-bottom: 20px;
    `;

    useEffect(() => {
        fetchBotConfiguration();
    }, []);

    const fetchBotConfiguration = () => {
        setIsLoading(true);

        fetch(`${config.apiBaseUrl}/botconfig/`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                setSelectedModel(data.model);
                setMaxTokens(data.maxTokens);
                setTemperature(data.temperature);
                setSystemPrompt(data.systemPrompt);
            })
            .catch(error => {
                console.error('There was an error!', error);
                setError(error.toString());
            })
            .finally(() => setIsLoading(false));
    };

    const handleInputChange = (value, endpoint) => {
        clearTimeout(debounceTimerRef.current);

        debounceTimerRef.current = setTimeout(() => {
            const requestData = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(value),
            };

            fetch(`${config.apiBaseUrl}/${endpoint}`, requestData)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                })
                .catch(error => {
                    console.error('There was an error!', error);
                    setError(error.toString());
                });
        }, 500);
    };

    const handleTemperatureChange = event => {
        const newTemperature = parseFloat(event.target.value);
        setTemperature(newTemperature);
        handleInputChange(newTemperature, 'temperature');
    };

    const handleMaxTokensChange = event => {
        const newMaxTokens = parseInt(event.target.value);
        setMaxTokens(newMaxTokens);
        handleInputChange(newMaxTokens, 'maxtokens');
    };

    const handleSystemPromptChange = event => {
        const newSystemPrompt = event.target.value;
        setSystemPrompt(newSystemPrompt);
        handleInputChange(newSystemPrompt, 'systemprompt');
    };

    const handleModelChange = event => {
        const newModel = event.target.value;
        setSelectedModel(newModel);
        handleInputChange(newModel, 'model');
    };

    return (
        <CollapsibleContainer title="OpenAI Configuration" width='450px'>
            {isLoading ? (
                <p>Loading...</p>
            ) : (
                <>
                    <Label>System Prompt</Label>
                    <TextArea
                        value={systemPrompt}
                        onChange={handleSystemPromptChange}
                    />
                    <SliderContainer>
                        <Label>Temperature: {temperature}</Label>
                        <input
                            type="range"
                            min={0}
                            max={1}
                            step={0.01}
                            value={temperature}
                            onChange={handleTemperatureChange}
                        />
                    </SliderContainer>
                    <Label>Max Tokens</Label>
                    <Input
                        type="number"
                        value={maxTokens}
                        onChange={handleMaxTokensChange}
                    />
                    <Label>Model</Label>
                    <Select id="model" value={selectedModel} onChange={handleModelChange}>
                        {modelOptions.map(option => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </Select>
                </>
            )}
        </CollapsibleContainer>
    );
};

export default AIConfigurationForm;
