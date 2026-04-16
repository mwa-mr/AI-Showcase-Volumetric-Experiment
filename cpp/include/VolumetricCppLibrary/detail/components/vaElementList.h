#pragma once

namespace va {
    class Element;

    namespace detail {
        class ElementList : public va::NonMovable {
        public:
            ~ElementList() {
                Clear();
            }

            std::vector<Element*> Children() const {
                std::vector<Element*> result;
                std::lock_guard<std::mutex> lock(m_mutex);
                for (auto& it : m_children) {
                    result.push_back(it.get());
                }
                return result;
            }

            Element* AddElement(std::unique_ptr<Element>&& element) {
                std::lock_guard<std::mutex> lock(m_mutex);
                m_children.emplace_back(std::move(element));
                return m_children.back().get();
            }

            void RemoveElement(Element* element) {
                std::lock_guard<std::mutex> lock(m_mutex);
                m_children.erase(std::remove_if(m_children.begin(), m_children.end(), [&](const std::unique_ptr<va::Element>& e) { return e.get() == element; }), m_children.end());
            }

            void Clear() {
                std::lock_guard<std::mutex> lock(m_mutex);
                // Destroy elements in reverse order of creation to ensure dependent elements
                // (children, references) are destroyed before the elements they depend on.
                while (!m_children.empty()) {
                    m_children.pop_back();
                }
            }

            Element* GetElementOrThrow(VaElement element) {
                std::lock_guard<std::mutex> lock(m_mutex);
                auto it = std::find_if(m_children.begin(), m_children.end(), [&](const auto& e) { return e->ElementHandle() == element; });
                if (it == m_children.end()) {
                    THROW("Element not found.");
                }
                return it->get();
            }

            bool TryGetElement(VaElement element, Element** outElement) {
                std::lock_guard<std::mutex> lock(m_mutex);
                auto it = std::find_if(m_children.begin(), m_children.end(), [&](const auto& e) { return e->ElementHandle() == element; });
                if (it == m_children.end()) {
                    return false;
                }
                *outElement = it->get();
                return true;
            }

        private:
            mutable std::mutex m_mutex;
            std::vector<std::unique_ptr<va::Element>> m_children;
        };
    } // namespace detail
} // namespace va
