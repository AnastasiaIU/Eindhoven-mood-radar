\# MoodRadar – ML Models



MoodRadar is a machine learning project that experiments with different models to predict or classify user mood based on sensor and contextual data. This repository contains the project documentation and two trained models: an initial Random Forest baseline and a final Gradient Boosting model.



\## Repository structure



\- `documents/` – project documentation (model design, AI model documentation, user stories, dataset research, sensor research, training data labelling guide, GDPR/AI Act compliance checklist, API integration plan).

\- `random\_forest/` – first baseline model using Random Forest (training notebook and results).

\- `gradient\_boosting/` – final model using Gradient Boosting (training notebook and results).



\## Model development history



The Random Forest model was developed as the first attempt and used as a baseline to understand data behaviour and obtain an initial performance benchmark.



After analysing the baseline, a Gradient Boosting model was trained and tuned. This model achieved better performance and is considered the final model for the project.



\## How to run the notebooks



1\. Clone this repository:

&#x20;  ```bash

&#x20;  git clone <YOUR\_REPO\_URL>

&#x20;  cd <YOUR\_REPO\_NAME>

&#x20;  ```

2\. (Optional) Create and activate a virtual environment.

3\. Install dependencies:

&#x20;  ```bash

&#x20;  pip install -r requirements.txt

&#x20;  ```

4\. Start Jupyter:

&#x20;  ```bash

&#x20;  jupyter notebook

&#x20;  ```

5\. Open and run:

&#x20;  - `random\_forest/random\_forest\_model.ipynb` for the baseline model.

&#x20;  - `gradient\_boosting/gradient\_boosting\_model.ipynb` for the final model.



Make sure the notebook paths to the dataset are correct for your local setup.



\## Requirements



Core Python libraries used (see `requirements.txt` for full list):



\- numpy

\- pandas

\- scikit-learn

\- jupyter

\- matplotlib / seaborn (for plots, if used in notebooks)



\## License and usage



This project is intended for educational and research purposes.  

You may adapt the code and documentation as needed for your own learning or internal projects, respecting any restrictions attached to the original datasets used.

