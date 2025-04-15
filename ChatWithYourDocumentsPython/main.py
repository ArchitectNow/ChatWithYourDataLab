import os
import logging
from semantic_kernel import Kernel
from semantic_kernel.utils.logging import setup_logging
from semantic_kernel.functions import kernel_function
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.connectors.ai.function_choice_behavior import FunctionChoiceBehavior
from semantic_kernel.connectors.ai.chat_completion_client_base import ChatCompletionClientBase
from semantic_kernel.contents.chat_history import ChatHistory
from semantic_kernel.functions.kernel_arguments import KernelArguments
from fastapi import FastAPI, HTTPException, Depends
from pydantic import BaseModel
from dotenv import load_dotenv

from azure.search.documents import SearchClient
from azure.core.credentials import AzureKeyCredential
from semantic_kernel.connectors.ai.open_ai.prompt_execution_settings.azure_chat_prompt_execution_settings import (
    AzureChatPromptExecutionSettings,
)
from azure.search.documents.models import QueryType

# Load environment variables
load_dotenv()

# Azure OpenAI Config
AZURE_OPENAI_ENDPOINT = os.getenv("AZURE_OPENAI_ENDPOINT")
AZURE_OPENAI_API_KEY = os.getenv("AZURE_OPENAI_API_KEY")
AZURE_OPENAI_DEPLOYMENT = os.getenv("AZURE_OPENAI_DEPLOYMENT")

# Azure Cognitive Search Config
AZURE_SEARCH_ENDPOINT = os.getenv("AZURE_SEARCH_ENDPOINT")
AZURE_SEARCH_API_KEY = os.getenv("AZURE_SEARCH_API_KEY")
AZURE_SEARCH_INDEX = os.getenv("AZURE_SEARCH_INDEX")

# FastAPI app
app = FastAPI()

# Azure Cognitive Search Client
search_client = SearchClient(
    endpoint=AZURE_SEARCH_ENDPOINT,
    index_name=AZURE_SEARCH_INDEX,
    credential=AzureKeyCredential(AZURE_SEARCH_API_KEY)
)


# Initialize Semantic Kernel
kernel = Kernel()

chat_completion = AzureChatCompletion(
    service_id="azure_openai_chat",
    deployment_name=AZURE_OPENAI_DEPLOYMENT,
    api_key=AZURE_OPENAI_API_KEY,
    base_url=AZURE_OPENAI_ENDPOINT,
)

kernel.add_service(chat_completion)


class ChatRequest(BaseModel):
    query: str


def search_documents(query: str) -> str:
    """Search for relevant documents in Azure Cognitive Search using semantic config and score filtering."""

    results = search_client.search(
        search_text=query,
        query_type=QueryType.SEMANTIC,
        semantic_configuration_name="SemanticConfig",
        top=2,
        include_total_count=False
    )

    content_lines = []
    for result in results:
        # result has .score, and its fields can be accessed like a dict
        print(result)
        if result["@search.score"] and result["@search.score"] > 5 and "content" in result:
            content_lines.append(result["content"])

    return "\n".join(content_lines) if content_lines else "No relevant documents found."


@app.post("/api/chat")
async def chat(request: ChatRequest):
    """Processes chat requests and returns AI-generated responses using Semantic Kernel."""
    try:
         # Retrieve document context
        document_context = search_documents(request.query)
        # Enhance AI prompt with document context
        ai_prompt = f"Use the following document context to answer: {document_context}\n\nUser: {request.query}"

        history = ChatHistory()
        history.add_user_message(ai_prompt)
        result = await chat_completion.get_chat_message_content(
            chat_history=history,
            settings=AzureChatPromptExecutionSettings(),
            kernel=kernel,
        )
        
        return {"response": str(result)}
    except Exception as e:
        print(e)
        raise HTTPException(status_code=500, detail=str(e))
