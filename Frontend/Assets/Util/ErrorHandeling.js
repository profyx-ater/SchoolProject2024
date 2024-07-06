export function displayError(error, errorOutputElement) {
        let errorMessage;

        if (error.message){
            errorMessage = error.message;
        }else if(error){
            errorMessage = error;
        }
        if (errorOutputElement) {
            errorOutputElement.textContent = errorMessage;
            setTimeout(() => { errorOutputElement.textContent = ""; }, 10000);
        }

}