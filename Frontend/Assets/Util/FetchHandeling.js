// We gaan deze functie gebruiken om alle fetch verzoeken te behandelen
export async function fetchData(url, options){
    try{
        const response = await fetch(url, options);
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || errorData.title || errorData || 'Network response was not OK');
        }
        const text = await response.text();
        if (text) {
            return JSON.parse(text);
        } else {
            return {};
        }
    }catch (error){
        console.log(error)
        throw error;
    }
}